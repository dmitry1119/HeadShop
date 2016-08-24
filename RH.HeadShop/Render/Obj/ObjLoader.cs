using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using RH.HeadShop.Helpers;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Helpers;

namespace RH.HeadShop.Render.Obj
{
    public static class ObjLoader
    {
        public static ObjItem LoadObjFile(string filePath)
        {
            return LoadObjFile(filePath, false);
        }

        public static ObjItem LoadObjFile(string filePath, bool needExporter)
        {
            var result = new ObjItem(needExporter);
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                return null;

            using (var sr = new StreamReader(fi.FullName, Encoding.Default))
            {
                var currentGroup = default(ObjGroup);
                CheckAndAttachDefaultGroup(ref currentGroup, ref result);

                var index = 0;
                var lastGroupName = String.Empty;

                if (ProgramCore.PluginMode)
                {
                    var folderPath = Path.Combine(Application.StartupPath, "Models\\Model", ProgramCore.Project.ManType.GetObjDirPath());
                    switch (ProgramCore.Project.ManType)
                    {
                        case ManType.Male:
                            LoadMtlib(Path.Combine(folderPath, "Male.mtl"), ref result);
                            break;
                        case ManType.Female:
                            LoadMtlib(Path.Combine(folderPath, "Fem.mtl"), ref result);
                            break;
                        case ManType.Child:
                            LoadMtlib(Path.Combine(folderPath, "Child.mtl"), ref result);
                            break;
                    }
                }

                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();

                    if (String.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
                    {
                        if (currentLine == "#Accessories")
                            result.accessoryByHeadShop = true;
                        else if (currentLine == "#HeadShop Model")
                            result.modelByHeadShop = true;
                        continue;
                    }

                    var fields = currentLine.Trim().Split(null, 2);
                    if (fields.Length < 2)
                    {
                        ProgramCore.EchoToLog(String.Format("Bad obj file format. File: '{0}'", fi.FullName), EchoMessageType.Warning);
                        continue;
                    }

                    var keyword = fields[0].Trim().ToLower();
                    var data = fields[1].Trim();

                    switch (keyword)
                    {
                        case "v":       // verticles
                            var vertex = ParseVector3(data);
                            result.Vertices.Add(vertex);
                            if (needExporter)
                                result.ObjExport.Vertices.Add(vertex);
                            break;
                        case "vt":      // texture coords
                            var textureCoord = ParseTextureCoords(data);
                            result.TextureCoords.Add(textureCoord);
                            if (needExporter)
                                result.ObjExport.TexCoords.Add(textureCoord);
                            break;
                        case "vn":      // normals
                            var normal = ParseVector3(data);
                            result.Normals.Add(normal);
                            if (needExporter)
                                result.ObjExport.Normals.Add(normal);
                            break;
                        case "f":      // faces
                            var face = ParceFace(data);
                            if (needExporter)
                            {
                                face.ObjExportIndex = result.ObjExport.Faces.Count;
                                result.ObjExport.Faces.Add(new ObjExportFace(face.Count, face.Vertices));
                            }
                            currentGroup.AddFace(face);
                            index++;
                            break;
                        case "g":      // start group
                            if (needExporter)
                            {
                                lastGroupName = data;
                                if (result.ObjExport.MaterialsGroups.Count > 0)
                                {
                                    result.ObjExport.MaterialsGroups.Last().Groups.Last().EndFaceIndex = index - 1;
                                    result.ObjExport.MaterialsGroups.Last().Groups.Add(new ObjExportGroup
                                    {
                                        Group = data,
                                        StartFaceIndex = index
                                    });
                                }
                            }
                            break;
                        case "mtllib":  //parse mtl file
                            var path = Path.Combine(fi.DirectoryName, data);
                            LoadMtlib(path, ref result);
                            break;
                        case "usemtl":
                            if (needExporter)
                            {
                                if (result.ObjExport.MaterialsGroups.Count > 0)
                                    result.ObjExport.MaterialsGroups.Last().Groups.Last().EndFaceIndex = index - 1;
                                result.ObjExport.MaterialsGroups.Add(new ObjExportMaterial
                                {
                                    Material = data,
                                    Groups = new List<ObjExportGroup> 
                                        {
                                            new ObjExportGroup
                                            {
                                                Group = lastGroupName,
                                                StartFaceIndex = index
                                            }
                                        }
                                });
                            }

                            var lowerData = data.ToLower();
                            var materialKey = result.Materials.Keys.SingleOrDefault(x => x == lowerData);
                            ObjMaterial material;
                            if (materialKey == null)                        // if can't parse mtl, create default group
                            {
                                material = new ObjMaterial(lowerData);
                                result.Materials.Add(lowerData, material);
                            }
                            else
                                material = result.Materials[materialKey];

                            if (result.Groups.ContainsKey(material))
                                currentGroup = result.Groups[material];
                            else
                            {
                                currentGroup = new ObjGroup(material.Name);
                                result.Groups.Add(material, currentGroup);
                            }
                            break;
                    }
                }
                if (result.ObjExport != null && result.ObjExport.MaterialsGroups.Count > 0 && needExporter)
                    result.ObjExport.MaterialsGroups.Last().Groups.Last().EndFaceIndex = index - 1;
            }
            return result;
        }

        private static void CheckAndAttachDefaultGroup(ref ObjGroup currentGroup, ref ObjItem result)
        {
            if (currentGroup == null)
            {
                currentGroup = new ObjGroup("default");
                var defaultGroupName = "HeadShopDefaultMaterial".ToLower();
                var defaultMaterial = new ObjMaterial(defaultGroupName);
                result.Groups.Add(defaultMaterial, currentGroup);
                result.Materials.Add(defaultGroupName, defaultMaterial);
            }
        }

        private static ObjFace ParceFace(string line)
        {
            var vertices = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var face = new ObjFace();

            foreach (var vertexString in vertices)
            {
                var faceVertex = ParseFaceVertex(vertexString);
                face.AddVertex(faceVertex);
            }

            return face;
        }

        private static Vector3 ParseVector3(string line)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var x = StringConverter.ToFloat(parts[0]);
            var y = StringConverter.ToFloat(parts[1]);
            var z = StringConverter.ToFloat(parts[2]);

            return new Vector3(x, y, z);
        }

        private static ObjFaceVertex ParseFaceVertex(string vertexString)
        {
            var fields = vertexString.Split(new[] { '/' }, StringSplitOptions.None);

            var vertexIndex = StringConverter.ToInt(fields[0]);
            var faceVertex = new ObjFaceVertex(vertexIndex, 0, 0);

            if (fields.Length > 1)
            {
                var textureIndex = fields[1].Length == 0 ? 0 : StringConverter.ToInt(fields[1]);
                faceVertex.TextureIndex = textureIndex;
            }

            if (fields.Length > 2)
            {
                var normalIndex = fields.Length > 2 && fields[2].Length == 0 ? 0 : StringConverter.ToInt(fields[2]);
                faceVertex.NormalIndex = normalIndex;
            }

            return faceVertex;
        }

        private static Vector2 ParseTextureCoords(string line)
        {
            var parts = line.Split(' ');

            var x = StringConverter.ToFloat(parts[0]);
            var y = StringConverter.ToFloat(parts[1]);

            return new Vector2(x, y);
        }

        private static void LoadMtlib(string filePath, ref ObjItem result)
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                return;

            using (var sr = new StreamReader(fi.FullName))
            {
                var material = default(ObjMaterial);
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();

                    if (String.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
                        continue;

                    var fields = currentLine.Trim().Split(null, 2);
                    var keyword = fields[0].Trim().ToLower();
                    var data = fields[1].Trim();

                    switch (keyword)
                    {
                        case "newmtl":
                            var dt = data.ToLower();
                            if (result.Materials.ContainsKey(dt))
                                material = result.Materials[dt];
                            else
                            {
                                material = new ObjMaterial(data);
                                result.Materials.Add(dt, material);
                            }
                            break;
                        case "ka":
                            if (material != null)
                                material.AmbientColor = ParseVector3(data);
                            break;
                        case "kd":
                            if (material != null)
                            {
                                var color = ParseVector3(data);
                                material.DiffuseColor = new Vector4(color.X, color.Y, color.Z, 1f);
                            }
                            break;
                        case "ks":
                            if (material != null)
                                material.SpecularColor = ParseVector3(data);
                            break;
                        case "ns":
                            if (material != null)
                                material.SpecularCoefficient = StringConverter.ToFloat(data);
                            break;
                        case "ni":
                            if (material != null)
                                material.OpticalDensity = StringConverter.ToFloat(data);
                            break;
                        case "d":
                        case "tr":
                            if (material != null)
                                material.Transparency = StringConverter.ToFloat(data);
                            break;
                        case "illum":
                            if (material != null)
                                material.IlluminationModel = StringConverter.ToInt(data);
                            break;
                        case "map_ka":
                            if (material != null)
                                material.AmbientTextureMap = GetMapFullPath(data, filePath);
                            break;
                        case "map_kd":
                            if (material != null)
                                material.DiffuseTextureMap = GetMapFullPath(data, filePath);
                            break;
                        case "map_ks":
                            if (material != null)
                                material.SpecularTextureMap = GetMapFullPath(data, filePath);
                            break;
                        case "map_ns":
                            if (material != null)
                                material.SpecularHighlightTextureMap = GetMapFullPath(data, filePath);
                            break;
                        case "map_d":
                            if (material != null)
                                material.TransparentTextureMap = GetMapFullPath(data, filePath);
                            break;
                        case "map_bump":
                        case "bump":
                            if (material != null)
                                material.BumpMap = GetMapFullPath(data, filePath);
                            break;
                        case "disp":
                            if (material != null)
                                material.DisplacementMap = GetMapFullPath(data, filePath);
                            break;
                        case "decal":
                            if (material != null)
                                material.StencilDecalMap = GetMapFullPath(data, filePath);
                            break;
                    }
                }
            }
        }

        private static string GetMapFullPath(string data, string mtlPath)
        {
            if (data.StartsWith("/"))       // относительные пути
            {
                var path = data.Replace("/", "\\").Remove(0, 1);
                var directory = Path.GetDirectoryName(mtlPath);
                return Path.Combine(directory, path);
            }

            return data;
        }

        /// <summary> Get all used in mtl elements</summary>
        public static HashSet<string> GetMtlibUsingItems(string filePath)
        {
            var result = new HashSet<string>();

            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                return result;

            using (var sr = new StreamReader(fi.FullName))
            {
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();

                    if (String.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
                        continue;

                    var fields = currentLine.Trim().Split(null, 2);
                    var keyword = fields[0].Trim().ToLower();
                    var data = fields[1].Trim();

                    switch (keyword)
                    {
                        case "map_ka":
                        case "map_ks":
                        case "map_ns":
                        case "map_d":
                        case "map_bump":
                        case "bump":
                        case "disp":
                        case "decal":
                        case "map_kd":                  // we need only diffuse
                            result.Add(GetMapFullPath(data, filePath));
                            break;
                    }
                }
            }
            return result;
        }

        public static void AppendObjTriangle(ObjItem objModel, ObjFaceVertex faceVertex,
                                             ref List<float> vertexPositions, ref List<float> vertexNormals, ref List<float> vertexTextureCoordinates, ref List<float> vertexBoneWeights, ref List<float> vertexBoneIndices, ref List<uint> indeces)
        {
            if (indeces != null)
                indeces.Add((uint)(vertexPositions.Count / 3));
            if (vertexPositions != null)
            {
                var vertexPosition = objModel.Vertices[faceVertex.VertexIndex - 1];
                vertexPositions.Add(vertexPosition.X);
                vertexPositions.Add(vertexPosition.Y);
                vertexPositions.Add(vertexPosition.Z);
            }
            if (vertexNormals != null)
            {
                if (objModel.Normals.Count == 0)
                {
                    vertexNormals.AddRange(new[] { 0.0f, 0.0f, 0.0f });
                }
                else
                {
                    var vertexNormal = objModel.Normals[faceVertex.NormalIndex - 1];
                    vertexNormals.Add(vertexNormal.X);
                    vertexNormals.Add(vertexNormal.Y);
                    vertexNormals.Add(vertexNormal.Z);
                }
            }
            if (vertexTextureCoordinates != null)
            {
                var vertexTexture = objModel.TextureCoords[faceVertex.TextureIndex - 1];
                vertexTextureCoordinates.Add(vertexTexture.X);
                vertexTextureCoordinates.Add(vertexTexture.Y);
            }
            if (vertexBoneIndices != null)
                vertexBoneIndices.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
            if (vertexBoneWeights != null)
                vertexBoneWeights.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
        }

        public static void CopyMtl(string mtl, string newMtlName, string path, string subFolders, string projectPath)
        {
            var mtlPath = Path.Combine(path, mtl);
            var mtlFi = new FileInfo(mtlPath);
            if (mtlFi.Exists)
            {
                var newMtlPath = Path.Combine(projectPath, subFolders, newMtlName);
                File.Copy(mtlPath, newMtlPath, true);

                var diffuseTextures = GetMtlibUsingItems(mtlPath);
                foreach (var texturePath in diffuseTextures)
                {
                    if (!File.Exists(texturePath))
                        continue;

                    var relativeTexturePath = texturePath.Remove(0, path.Length);
                    var newTexturePath = relativeTexturePath.Replace("/", "\\");       // format of mtl
                    newTexturePath = newTexturePath.StartsWith("\\") ? newTexturePath.Remove(0, 1) : newTexturePath;
                    newTexturePath = Path.Combine(projectPath, subFolders, newTexturePath);
                    var textureFi = new FileInfo(newTexturePath);
                    if (texturePath != textureFi.FullName)
                    {
                        FolderEx.CreateDirectory(textureFi.Directory); // recreate folders structure
                        File.Copy(texturePath, textureFi.FullName, true);

                        using (var ms = new MemoryStream(File.ReadAllBytes(textureFi.FullName))) // Don't use using!!
                        {
                            var img = (Bitmap)Bitmap.FromStream(ms);
                            var max = Math.Max(img.Width, img.Height);
                            if (max > 1024)
                            {
                                var k = 1024.0f / max;
                                var newImg = ImageEx.ResizeImage(img, new Size((int)(img.Width * k), (int)(img.Height * k)));
                                newImg.Save(textureFi.FullName, ImageFormat.Jpeg);
                            }
                        }
                    }
                }
            }
        }
    }
}
