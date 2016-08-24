using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using RH.HeadShop.Helpers;
using RH.HeadShop.Render.Helpers;
using RH.HeadShop.Render.Meshes;
using RH.HeadEditor.Data;

namespace RH.HeadShop.Render.Obj
{
    public static class ObjSaver
    {
        public static void SaveObjFile(string filePath, RenderMesh mesh, MeshType type)
        {
            SaveObjFile(filePath, mesh, type, null);
        }
        public static void SaveObjFile(string filePath, RenderMesh mesh, MeshType type, ObjExport objExport)
        {
            var meshInfos = new List<MeshInfo>();

            foreach (var part in mesh.Parts)
                meshInfos.Add(new MeshInfo(part));

            SaveObjFile(filePath, meshInfos, type, objExport, mesh);
        }
        public static void SaveObjFile(string filePath, DynamicRenderMeshes meshes, MeshType type)
        {
            var meshInfos = new List<MeshInfo>();

            foreach (var mesh in meshes)
            {
                var meshInfo = mesh.GetMeshInfo();
                meshInfos.Add(meshInfo);
            }
            SaveObjFile(filePath, meshInfos, type);
        }
        public static void SaveObjFile(string filePath, List<MeshInfo> meshInfos, MeshType type, ObjExport objExport, RenderMesh mesh)
        {
            if (objExport == null)
            {
                SaveObjFile(filePath, meshInfos, type);
                return;
            }
            var fi = new FileInfo(filePath);
            if (fi.Exists)
                fi.Delete();

            if (fi.DirectoryName == null) return;
            var mtlPath = Path.Combine(fi.DirectoryName, Path.GetFileNameWithoutExtension(fi.Name) + ".mtl");
            var fiMtl = new FileInfo(mtlPath);
            //if (fiMtl.Exists)
            //    fiMtl.Delete();
            var groupIndex = 0;
            var materials = new Dictionary<string, ObjMaterial>();
            var startPositionIndex = 1;
            var startTexIndex = 1;
            var startNormalIndex = 1;
            var indicesPositions = new List<int>();
            var indicesTexCoords = new List<int>();
            var indicesNormals = new List<int>();
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var texCoords = new List<Vector2>();

            foreach (var meshInfo in meshInfos) // faces should write differently
            {
                if (meshInfo.IndicesNormals.Count == 0)
                    continue;

                String groupTitle;
                if (string.IsNullOrEmpty(meshInfo.Title))
                {
                    groupTitle = "Element_" + groupIndex;
                    while (materials.ContainsKey(groupTitle))
                    {
                        ++groupIndex;
                        groupTitle = "Element_" + groupIndex;
                    }
                    ++groupIndex;
                }
                else
                    groupTitle = meshInfo.Title;
                materials.Add(groupTitle, meshInfo.Material);

                for (var i = 0; i < meshInfo.IndicesTexCoords.Count; i++)
                {
                    indicesPositions.Add(startPositionIndex + meshInfo.IndicesPositions[i]);
                    indicesTexCoords.Add(startTexIndex + meshInfo.IndicesTexCoords[i]);
                    indicesNormals.Add(startNormalIndex + meshInfo.IndicesNormals[i]);
                }

                startPositionIndex += (meshInfo.IndicesPositions.Max() + 1);
                startTexIndex += (meshInfo.IndicesTexCoords.Max() + 1);
                startNormalIndex += (meshInfo.IndicesNormals.Max() + 1);

                positions.AddRange(meshInfo.Positions);
                normals.AddRange(meshInfo.Normals);
                texCoords.AddRange(meshInfo.TexCoords);
            }

            foreach (var face in objExport.Faces)
            {
                var index = face.TriangleIndex0 * 3;

                for (int l = 0; l < 3; l++)
                {
                    var v = positions[indicesPositions[index + l] - 1];
                    var vn = normals[indicesNormals[index + l] - 1];
                    var vt = texCoords[indicesTexCoords[index + l] - 1];
                    objExport.SetData(v, vt, vn, face, l);
                }
                if (face.VertexCount == 4)
                {
                    index = face.TriangleIndex1 * 3;
                    var v = positions[indicesPositions[index + 1] - 1];
                    var vn = normals[indicesNormals[index + 1] - 1];
                    var vt = texCoords[indicesTexCoords[index + 1] - 1];
                    objExport.SetData(v, vt, vn, face, 3);
                }
            }

            using (var sw = new StreamWriter(filePath, false, Encoding.Default))
            {
                sw.WriteLine("#Produced by HeadShop");
                sw.WriteLine(type == MeshType.Accessory ? "#Accessories" : "#HeadShop Model");
                sw.WriteLine("#" + DateTime.Now.ToShortDateString());

                sw.WriteLine("mtllib " + fiMtl.Name);
                sw.WriteLine();

                foreach (var v in objExport.Vertices)
                {
                    var resStr = "v " + v.X.ToString(ProgramCore.Nfi) + " " + v.Y.ToString(ProgramCore.Nfi) + " " +
                                 v.Z.ToString(ProgramCore.Nfi);
                    sw.WriteLine(resStr);
                }

                foreach (var vt in objExport.TexCoords)
                {
                    var resStr = "vt " + vt.X.ToString(ProgramCore.Nfi) + " " + vt.Y.ToString(ProgramCore.Nfi);
                    sw.WriteLine(resStr);
                }

                foreach (var vn in objExport.Normals)
                {
                    var resStr = "vn " + vn.X.ToString(ProgramCore.Nfi) + " " + vn.Y.ToString(ProgramCore.Nfi) + " " +
                                 vn.Z.ToString(ProgramCore.Nfi);
                    sw.WriteLine(resStr);
                }

                String prevGroup = String.Empty;
                foreach (var materialGroup in objExport.MaterialsGroups)
                {
                    if (materialGroup.Groups.Count == 0 || !materialGroup.Groups[0].IsValid)
                        continue;
                    for (int i = 0; i < materialGroup.Groups.Count; i++)
                    {
                        var group = materialGroup.Groups[i];
                        if (!group.IsValid)
                            continue;
                        if (!prevGroup.Equals(group.Group))
                        {
                            sw.WriteLine("g " + group.Group);
                            prevGroup = group.Group;
                        }
                        if (i == 0)
                            sw.WriteLine("usemtl " + materialGroup.Material);
                        for (int j = group.StartFaceIndex; j <= group.EndFaceIndex; j++)
                        {
                            var face = objExport.Faces[j];
                            String resStr = "f ";
                            foreach (var vertex in face.FaceVertices)
                            {
                                resStr += (vertex.VertexIndex).ToString(ProgramCore.Nfi);
                                if (objExport.TexCoords.Count > 0)
                                    resStr += "/" + (vertex.TextureIndex).ToString(ProgramCore.Nfi);
                                if (objExport.Normals.Count > 0)
                                    resStr += "/" + (vertex.NormalIndex).ToString(ProgramCore.Nfi);
                                resStr += " ";
                            }

                            sw.WriteLine(resStr.Remove(resStr.Length - 1));
                        }
                    }
                }
            }
            // SaveMaterial(mtlPath, materials, fi);
        }
        public static void SaveObjFile(string filePath, List<MeshInfo> meshInfos, MeshType type)
        {
            if (meshInfos.Count == 0)
                return;

            var fi = new FileInfo(filePath);
            if (fi.Exists)
                fi.Delete();

            var mtlPath = Path.Combine(fi.DirectoryName, Path.GetFileNameWithoutExtension(fi.Name) + ".mtl");
            var fiMtl = new FileInfo(mtlPath);
            if (fiMtl.Exists)
                fiMtl.Delete();

            var materials = new Dictionary<string, ObjMaterial>();         // group title, diffuse color, texture path
            using (var sw = new StreamWriter(filePath, false, Encoding.Default))
            {
                sw.WriteLine("#Produced by HeadShop");
                if (type == MeshType.Accessory)
                    sw.WriteLine("#Accessories");
                else
                    sw.WriteLine("#HeadShop Model");
                sw.WriteLine("#" + DateTime.Now.ToShortDateString());

                sw.WriteLine("mtllib " + fiMtl.Name);
                sw.WriteLine();

                SaveVerticles(meshInfos, sw);
                SaveTextureCoords(meshInfos, sw);
                SaveNormals(meshInfos, sw);

                var groupIndex = 0;
                var startPositionIndex = 1;
                var startTexIndex = 1;
                var startNormalIndex = 1;
                foreach (var meshInfo in meshInfos)         // faces should write differently
                {
                    if (meshInfo.IndicesNormals.Count == 0)
                        continue;

                    var groupTitle = string.Empty;
                    if (string.IsNullOrEmpty(meshInfo.Title))
                    {
                        groupTitle = "Element_" + groupIndex;

                        while (materials.ContainsKey(groupTitle))
                        {
                            ++groupIndex;
                            groupTitle = "Element_" + groupIndex;
                        }

                        ++groupIndex;
                    }
                    else
                        groupTitle = meshInfo.Title;

                    materials.Add(groupTitle, meshInfo.Material);

                    sw.WriteLine("g " + groupTitle);
                    sw.WriteLine("usemtl " + groupTitle);

                    #region Faces

                    var resStr = "f ";
                    var index = 0;
                    for (var i = 0; i < meshInfo.IndicesTexCoords.Count; i++)
                    {
                        resStr += (startPositionIndex + meshInfo.IndicesPositions[i]).ToString(ProgramCore.Nfi) + "/";
                        resStr += (startTexIndex + meshInfo.IndicesTexCoords[i]).ToString(ProgramCore.Nfi) + "/";
                        resStr += (startNormalIndex + meshInfo.IndicesNormals[i]).ToString(ProgramCore.Nfi) + " ";
                        ++index;

                        if (index == 3)
                        {
                            index = 0;
                            sw.WriteLine(resStr.Remove(resStr.Length - 1));
                            resStr = "f ";
                        }
                    }

                    startPositionIndex += (meshInfo.IndicesPositions.Max() + 1);
                    startTexIndex += (meshInfo.IndicesTexCoords.Max() + 1);
                    startNormalIndex += (meshInfo.IndicesNormals.Max() + 1);

                    #endregion
                }
            }
            SaveMaterial(mtlPath, materials, fi);
        }

        private static void SaveMaterial(string mtlPath, Dictionary<string, ObjMaterial> materials, FileInfo fi)
        {
            using (var sw = new StreamWriter(mtlPath, false, Encoding.Default))
            {
                var res1 = string.Empty;
                foreach (var material in materials)
                {
                    res1 += "newmtl " + material.Key + "\n";

                    if (!float.IsNaN(material.Value.SpecularCoefficient))
                        res1 += "Ns " + material.Value.SpecularCoefficient.ToString(ProgramCore.Nfi) + "\n";
                    if (!float.IsNaN(material.Value.OpticalDensity))
                        res1 += "Ni " + material.Value.OpticalDensity.ToString(ProgramCore.Nfi) + "\n";

                    if (material.Value.AmbientColor != Vector3.Zero)
                        res1 += "Ka " + material.Value.AmbientColor.X.ToString(ProgramCore.Nfi) + " " + material.Value.AmbientColor.Y.ToString(ProgramCore.Nfi) + " " + material.Value.AmbientColor.Z.ToString(ProgramCore.Nfi) + "\n";
                    if (material.Value.DiffuseColor != Vector4.Zero)
                    {
                        res1 += "d " + material.Value.DiffuseColor.W.ToString(ProgramCore.Nfi) + "\n";
                        res1 += "Kd " + material.Value.DiffuseColor.X.ToString(ProgramCore.Nfi) + " " + material.Value.DiffuseColor.Y.ToString(ProgramCore.Nfi) + " " + material.Value.DiffuseColor.Z.ToString(ProgramCore.Nfi) + "\n";
                    }
                    if (material.Value.SpecularColor != Vector3.Zero)
                        res1 += "Ks " + material.Value.SpecularColor.X.ToString(ProgramCore.Nfi) + " " + material.Value.SpecularColor.Y.ToString(ProgramCore.Nfi) + " " + material.Value.SpecularColor.Z.ToString(ProgramCore.Nfi) + "\n";

                    SetTextureMap(material.Value.AmbientTextureMap, "map_Ka", fi, ref res1);
                    SetTextureMap(material.Value.DiffuseTextureMap, "map_Kd", fi, ref res1);
                    SetTextureMap(material.Value.SpecularTextureMap, "map_Ks", fi, ref res1);
                    SetTextureMap(material.Value.SpecularHighlightTextureMap, "map_Ns", fi, ref res1);
                    SetTextureMap(material.Value.TransparentTextureMap, "map_d", fi, ref res1);
                    SetTextureMap(material.Value.BumpMap, "map_Bump", fi, ref res1);
                    SetTextureMap(material.Value.DisplacementMap, "disp", fi, ref res1);
                    SetTextureMap(material.Value.StencilDecalMap, "decal", fi, ref res1);
                }
                sw.Write(res1);
            }
        }

        private static void SetTextureMap(string mapPath, string mapTitle, FileInfo fi, ref string res)
        {
            if (!string.IsNullOrEmpty(mapPath))
            {
                var textureName = Path.GetFileName(mapPath);
                var newTexturePath = Path.Combine(fi.DirectoryName, "Textures");
                var newTextureFullPath = Path.Combine(newTexturePath, textureName);

                if (!File.Exists(newTextureFullPath))
                {
                    FolderEx.CreateDirectory(new DirectoryInfo(newTexturePath));
                    File.Copy(mapPath, newTextureFullPath, false);
                }

                res += mapTitle + " /Textures/" + textureName + "\n";
            }
        }

        private static void SaveVerticles(IEnumerable<MeshInfo> meshInfos, StreamWriter sw)
        {
            foreach (var meshInfo in meshInfos)
            {
                foreach (var v in meshInfo.Positions)
                {
                    var resStr = "v " + v.X.ToString(ProgramCore.Nfi) + " " + v.Y.ToString(ProgramCore.Nfi) + " " + v.Z.ToString(ProgramCore.Nfi);
                    sw.WriteLine(resStr);
                }
            }
        }
        private static void SaveTextureCoords(IEnumerable<MeshInfo> meshInfos, StreamWriter sw)
        {
            foreach (var meshInfo in meshInfos)
            {
                foreach (var vt in meshInfo.TexCoords)
                {
                    var resStr = "vt " + vt.X.ToString(ProgramCore.Nfi) + " " + vt.Y.ToString(ProgramCore.Nfi);
                    sw.WriteLine(resStr);
                }
            }
        }
        private static void SaveNormals(IEnumerable<MeshInfo> meshInfos, StreamWriter sw)
        {
            foreach (var meshInfo in meshInfos)
            {
                foreach (var vn in meshInfo.Normals)
                {
                    var resStr = "vn " + vn.X.ToString(ProgramCore.Nfi) + " " + vn.Y.ToString(ProgramCore.Nfi) + " " + vn.Z.ToString(ProgramCore.Nfi);
                    sw.WriteLine(resStr);
                }
            }
        }
    }
}
