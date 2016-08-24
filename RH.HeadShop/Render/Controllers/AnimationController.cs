using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Assimp;
using Assimp.Configs;
using OpenTK;
using RH.HeadShop.IO;
using RH.HeadShop.Render.Helpers;
using RH.HeadShop.Render.Meshes;

namespace RH.HeadShop.Render.Controllers
{
    public class AnimationController
    {
        private static readonly Vector3D ZERO_VECTRO3D = new Vector3D(0.0f, 0.0f, 0.0f);
        private static readonly Assimp.Quaternion ZERO_QUATERNION = new Assimp.Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        private AnimatedNode rootNode;

        private AnimationInfo currentAnimation;

        public AnimationInfo СurrentAnimation
        {
            get
            {
                return currentAnimation;
            }
            set
            {
                currentAnimation = value;

                if (currentAnimation != null && !string.IsNullOrEmpty(currentAnimation.Sound))
                    ProgramCore.MainForm.ctrlRenderControl.soundController.Play(currentAnimation.Sound);
                else
                    ProgramCore.MainForm.ctrlRenderControl.soundController.Stop();
            }
        }

        public readonly Dictionary<string, AnimationInfo> Animations = new Dictionary<string, AnimationInfo>();
        public readonly Dictionary<string, AnimationInfo> Poses = new Dictionary<string, AnimationInfo>();
        private readonly Dictionary<String, int> bonesMapping = new Dictionary<string, int>();
        private Matrix4 m_globalInverseTransform = Matrix4.Identity;

        public List<BoneInfo> Bones = new List<BoneInfo>();
        public List<BaseRenderMesh> BodyMeshes = new List<BaseRenderMesh>();
        public Matrix4[] AnimationTransform = new Matrix4[80];

        public AnimationController()
        {
        }

        public bool Initialize(String daeFileName)
        {
            var fi = new FileInfo(daeFileName);
            if (!fi.Exists)
                return false;

            var scene = LoadDaeScene(fi.FullName);
            if (scene == null)
                return false;

            foreach (var t in BodyMeshes)
                t.Destroy();
            BodyMeshes.Clear();
            Bones.Clear();
            bonesMapping.Clear();
            Animations.Clear();
            Poses.Clear();
            rootNode = null;
            СurrentAnimation = null;

            m_globalInverseTransform = FromMatrix(scene.RootNode.Transform);
            m_globalInverseTransform.Invert();

            var title = Path.GetFileNameWithoutExtension(fi.FullName);
            LoadBodyMeshes(fi.FullName, scene, scene.RootNode, null);
            LoadBonesInfo(rootNode);
            LoadAnimations(scene, title);

            return true;
        }

        #region animation

        private int FindPosition(float animationTime, NodeAnimation channel)
        {
            for (var i = 0; i < channel.PositionKeysCount; i++)
                if (animationTime < channel.PositionKeys[i].Time)
                {
                    return i;
                }
            return 0;
        }
        private int FindRotation(float animationTime, NodeAnimation channel)
        {
            for (var i = 0; i < channel.RotationKeysCount; i++)
                if (animationTime < channel.RotationKeys[i].Time)
                {
                    return i;
                }
            return 0;
        }
        private int FindScaling(float animationTime, NodeAnimation channel)
        {
            for (var i = 0; i < channel.ScalingKeysCount; i++)
                if (animationTime < channel.ScalingKeys[i].Time)
                {
                    return i;
                }
            return 0;
        }

        private Vector3 CalcInterpolatedPosition(float animationTime, NodeAnimation channel)
        {
            if (channel.PositionKeysCount == 1)
            {
                return channel.PositionKeys[0].Value;
            }

            var positionIndex = FindPosition(animationTime, channel);
            var nextPositionIndex = positionIndex + 1;
            if (nextPositionIndex < channel.PositionKeysCount)
            {
                var positionKey = channel.PositionKeys[positionIndex];
                var nextPositionKey = channel.PositionKeys[nextPositionIndex];
                var deltaTime = nextPositionKey.Time - positionKey.Time;
                var factor = (animationTime - positionKey.Time) / deltaTime;
                return positionKey.Value + (nextPositionKey.Value - positionKey.Value) * factor;
            }
            return channel.PositionKeys[0].Value;
        }
        private Matrix4 CalcInterpolatedRotation(float animationTime, NodeAnimation channel)
        {
            Matrix4x4 matrix;
            if (channel.ScalingKeysCount == 1)
            {
                matrix = channel.RotationKeys[0].Value.GetMatrix();
            }

            var rotateIndex = FindRotation(animationTime, channel);
            var nextRotateIndex = rotateIndex + 1;
            if (nextRotateIndex < channel.RotationKeysCount)
            {
                var rotateKey = channel.RotationKeys[rotateIndex];
                var nextRotateKey = channel.RotationKeys[nextRotateIndex];
                var deltaTime = nextRotateKey.Time - rotateKey.Time;
                var factor = (animationTime - rotateKey.Time) / deltaTime;
                matrix = Assimp.Quaternion.Slerp(rotateKey.Value, nextRotateKey.Value, factor).GetMatrix();
            }
            else
            {
                matrix = channel.RotationKeys[0].Value.GetMatrix();
            }
            return FromMatrix(matrix);
        }
        private Vector3 CalcInterpolatedScaling(float animationTime, NodeAnimation channel)
        {
            if (channel.ScalingKeysCount == 1)
            {
                return channel.ScalingKeys[0].Value;
            }

            var scaleIndex = FindScaling(animationTime, channel);
            var nextScaleIndex = scaleIndex + 1;
            if (nextScaleIndex < channel.ScalingKeysCount)
            {
                var scaleKey = channel.ScalingKeys[scaleIndex];
                var nextScaleKey = channel.ScalingKeys[nextScaleIndex];
                var deltaTime = nextScaleKey.Time - scaleKey.Time;
                var factor = (animationTime - scaleKey.Time) / deltaTime;
                return scaleKey.Value + (nextScaleKey.Value - scaleKey.Value) * factor;
            }
            return channel.ScalingKeys[0].Value;
        }

        private void ReadNodeHeirarchy(float animationTime, AnimatedNode node, Matrix4 parentTransform)
        {
            var channel = СurrentAnimation.AnimationNodes.ContainsKey(node.Name) ? СurrentAnimation.AnimationNodes[node.Name] : null;
            var nodeTransformation = node.Transform;
            if (channel != null)
            {
                var scale = CalcInterpolatedScaling(animationTime, channel);
                var scaleMatrix = Matrix4.CreateScale(scale);

                var rotateMatrix = CalcInterpolatedRotation(animationTime, channel);

                var translate = CalcInterpolatedPosition(animationTime, channel);

                nodeTransformation = rotateMatrix * scaleMatrix;
                nodeTransformation.M14 = translate.X;
                nodeTransformation.M24 = translate.Y;
                nodeTransformation.M34 = translate.Z;
            }
            var globalTransformation = parentTransform * nodeTransformation;

            if (bonesMapping.ContainsKey(node.Name))
            {
                var boneIndex = bonesMapping[node.Name];
                Bones[boneIndex].FinalTransformation = m_globalInverseTransform * globalTransformation * Bones[boneIndex].BoneOffset;
                Bones[boneIndex].FinalTransformation.Transpose();
            }

            foreach (var child in node.Childs)
            {
                ReadNodeHeirarchy(animationTime, child, globalTransformation);
            }
        }

        public bool PoseBonesTransform(float animationTime)     // track value / 100f
        {
            if (СurrentAnimation == null)
                return false;
            ReadNodeHeirarchy(animationTime * СurrentAnimation.DurationInTicks, rootNode, Matrix4.Identity);

            for (var i = 0; i < Bones.Count; i++)
            {
                AnimationTransform[i] = Bones[i].FinalTransformation;
            }
            return true;
        }
        public bool BonesTransform(float timeInSeconds)
        {
            if (СurrentAnimation == null)
                return false;
            var ticksPerSecond = СurrentAnimation.TicksPerSecond != 0 ? СurrentAnimation.TicksPerSecond : 25.0f;
            var timeInTicks = timeInSeconds * ticksPerSecond;
            var animationTime = timeInTicks % СurrentAnimation.DurationInTicks;

            ReadNodeHeirarchy(animationTime, rootNode, Matrix4.Identity);

            for (var i = 0; i < Bones.Count; i++)
            {
                AnimationTransform[i] = Bones[i].FinalTransformation;
            }
            return true;
        }

        #endregion

        #region loading

        public void AddAnimations(String fileName)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                return;

            var scene = LoadDaeScene(fi.FullName);
            if (scene == null)
                return;

            var title = Path.GetFileNameWithoutExtension(fi.FullName);
            var animation = LoadAnimations(scene, title);

            if (animation != null)
            {
                var mp3Path = Path.Combine(fi.DirectoryName, title + ".mp3");
                var mp3Fi = new FileInfo(mp3Path);
                if (mp3Fi.Exists)                   // if contain sound - create link to it
                    animation.Sound = mp3Path;
            }
        }
        public void AddPoses(String fileName)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                return;

            var scene = LoadDaeScene(fileName);
            if (scene == null)
                return;

            var title = Path.GetFileNameWithoutExtension(fi.FullName);
            LoadAnimations(scene, title, true);
        }
        public void SetMorphPose(String fileName)
        {
        }

        private void FillAnimationNode(Scene scene, Node node, int animationIndex)
        {
            var animation = scene.Animations[animationIndex];
            NodeAnimationChannel channel = null;
            for (var i = 0; i < animation.NodeAnimationChannelCount; i++)
            {
                if (animation.NodeAnimationChannels[i].NodeName == node.Name)
                    channel = animation.NodeAnimationChannels[i];
            }
            if (channel != null && !СurrentAnimation.AnimationNodes.ContainsKey(node.Name))
            {
                var nodeAnim = new NodeAnimation
                {
                    PositionKeysCount = channel.PositionKeyCount,
                    ScalingKeysCount = channel.ScalingKeyCount,
                    RotationKeysCount = channel.RotationKeyCount
                };
                nodeAnim.PositionKeys = new VectorInfo[nodeAnim.PositionKeysCount];
                for (var i = 0; i < nodeAnim.PositionKeysCount; i++)
                    nodeAnim.PositionKeys[i] = new VectorInfo
                    {
                        Time = (float)channel.PositionKeys[i].Time,
                        Value = FromVector(channel.PositionKeys[i].Value)
                    };
                nodeAnim.RotationKeys = new QuaternionInfo[nodeAnim.RotationKeysCount];
                for (var i = 0; i < nodeAnim.RotationKeysCount; i++)
                    nodeAnim.RotationKeys[i] = new QuaternionInfo
                    {
                        Time = (float)channel.RotationKeys[i].Time,
                        Value = channel.RotationKeys[i].Value
                    };
                nodeAnim.ScalingKeys = new VectorInfo[nodeAnim.ScalingKeysCount];
                for (var i = 0; i < nodeAnim.ScalingKeysCount; i++)
                    nodeAnim.ScalingKeys[i] = new VectorInfo
                    {
                        Time = (float)channel.ScalingKeys[i].Time,
                        Value = FromVector(channel.ScalingKeys[i].Value)
                    };
                СurrentAnimation.AnimationNodes.Add(node.Name, nodeAnim);
            }
            for (var i = 0; i < node.ChildCount; i++)
                FillAnimationNode(scene, node.Children[i], animationIndex);
        }

        private AnimationInfo LoadAnimations(Scene scene, string title, bool pose = false)
        {
            var prevAnimation = СurrentAnimation;
            var animation = default(AnimationInfo);
            if (scene.AnimationCount == 0)
                return animation;
            const int firstAnimationIndex = 0;

            //for (int i = 0; i < scene.AnimationCount; i++)            // in one file always one animation
            {
                animation = new AnimationInfo(title, pose);
                СurrentAnimation = animation;
                animation.TicksPerSecond = (float)scene.Animations[firstAnimationIndex].TicksPerSecond;
                animation.DurationInTicks = (float)scene.Animations[firstAnimationIndex].DurationInTicks;
                FillAnimationNode(scene, scene.RootNode, firstAnimationIndex);
                if (pose)
                    Poses.Add(title, animation);
                else
                    Animations.Add(title, animation);
            }
            // СurrentAnimation = prevAnimation;
            return animation;
        }
        private void LoadBonesInfo(AnimatedNode node, BoneInfo parent = null)
        {
            if (bonesMapping.ContainsKey(node.Name))
            {
                var point = Vector3.Zero;
                var boneIndex = bonesMapping[node.Name];
                var bone = Bones[boneIndex];
                var transform = m_globalInverseTransform * bone.BoneOffset;
                transform.Transpose();
                bone.Point = -Vector3.Transform(point, transform);
                if (parent != null)
                {
                    var line = new BoneLineInfo
                    {
                        Point = bone.Point,
                        Direction = bone.Point - parent.Point
                    };
                    line.Length = line.Direction.Length;
                    line.Direction /= line.Length;
                    parent.Lines.Add(line);
                }
                foreach (var child in node.Childs)
                {
                    LoadBonesInfo(child, bone);
                }
            }
            else
            {
                foreach (var child in node.Childs)
                {
                    LoadBonesInfo(child, parent);
                }
            }
        }
        private void LoadBodyMeshes(string daePath, Scene scene, Node node, AnimatedNode animationNode)
        {
            if (node.HasMeshes)
            {
                var vertexPositions = new List<float>();
                var vertexNormals = new List<float>();
                var vertexTextureCoordinates = new List<float>();
                var vertexBoneIndices = new List<float>();
                var vertexBoneWeights = new List<float>();
                var indeces = new List<uint>();

                var fi = new FileInfo(daePath);
                foreach (var index in node.MeshIndices)
                {
                    var mesh = scene.Meshes[index];

                    var hasTexCoords = mesh.HasTextureCoords(0);

                    vertexPositions.Clear();
                    vertexNormals.Clear();
                    vertexTextureCoordinates.Clear();
                    vertexBoneIndices.Clear();
                    vertexBoneWeights.Clear();
                    indeces.Clear();

                    for (var i = 0; i < mesh.VertexCount; i++)
                    {
                        var vector = mesh.Vertices[i];
                        vertexPositions.Add(vector.X);
                        vertexPositions.Add(vector.Y);
                        vertexPositions.Add(vector.Z);

                        vector = mesh.HasNormals ? mesh.Normals[i] : ZERO_VECTRO3D;
                        vertexNormals.Add(vector.X);
                        vertexNormals.Add(vector.Y);
                        vertexNormals.Add(vector.Z);
                        vector = hasTexCoords ? mesh.GetTextureCoords(0)[i] : ZERO_VECTRO3D;
                        vertexTextureCoordinates.Add(vector.X);
                        vertexTextureCoordinates.Add(1.0f - vector.Y);
                        vertexBoneIndices.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
                        vertexBoneWeights.AddRange(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
                    }

                    foreach (var face in mesh.Faces)
                    {
                        indeces.Add(face.Indices[0]);
                        indeces.Add(face.Indices[1]);
                        indeces.Add(face.Indices[2]);
                    }

                    if (mesh.HasBones)
                    {
                        foreach (var bone in mesh.Bones)
                        {
                            if (!bone.HasVertexWeights)
                                continue;
                            int boneIndex;
                            if (bonesMapping.ContainsKey(bone.Name))
                            {
                                boneIndex = bonesMapping[bone.Name];
                            }
                            else
                            {
                                boneIndex = Bones.Count;
                                var boneInfo = new BoneInfo
                                                   {
                                                       BoneOffset = FromMatrix(bone.OffsetMatrix),
                                                   };
                                Bones.Add(boneInfo);
                                bonesMapping.Add(bone.Name, boneIndex);
                            }
                            foreach (var weight in bone.VertexWeights)
                            {
                                var vi = (int)weight.VertexID * 4;
                                for (var i = 0; i < 4; i++)
                                {
                                    if (vertexBoneWeights[vi + i] == 0.0f)
                                    {
                                        vertexBoneIndices[vi + i] = boneIndex;
                                        vertexBoneWeights[vi + i] = weight.Weight;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    var mtr = scene.Materials[mesh.MaterialIndex];

                    var texturePath = string.Empty;
                    var transparentPath = string.Empty;
                    if (mtr.GetTextureCount(TextureType.Diffuse) > 0)
                    {
                        var tex = mtr.GetTexture(TextureType.Diffuse, 0);
                        texturePath = Path.Combine(fi.DirectoryName, tex.FilePath);

                        transparentPath = Path.Combine(Path.GetDirectoryName(tex.FilePath), Path.GetFileNameWithoutExtension(tex.FilePath) + "_alpha" + Path.GetExtension(tex.FilePath));
                        transparentPath = File.Exists(transparentPath) ? transparentPath : string.Empty;
                    }

                    var renderMesh = new DynamicRenderMesh(MeshType.Head);
                    if (renderMesh.Create(vertexPositions, vertexNormals, vertexTextureCoordinates, vertexBoneIndices, vertexBoneWeights, indeces, texturePath, transparentPath))
                    {
                        renderMesh.Transform = FromMatrix(node.Transform);
                        renderMesh.Material.DiffuseColor = new Vector4(mtr.ColorDiffuse.R, mtr.ColorDiffuse.G, mtr.ColorDiffuse.B, mtr.ColorDiffuse.A);
                        BodyMeshes.Add(renderMesh);
                    }
                }
            }

            var parentNode = animationNode;
            if (animationNode == null)
            {
                rootNode = new AnimatedNode();
                parentNode = rootNode;
            }

            parentNode.Name = node.Name;
            parentNode.Transform = FromMatrix(node.Transform);

            for (var i = 0; i < node.ChildCount; i++)
            {
                var childNode = new AnimatedNode
                {
                    Parent = parentNode
                };
                LoadBodyMeshes(daePath, scene, node.Children[i], childNode);
                parentNode.Childs.Add(childNode);
            }
        }
        private Scene LoadDaeScene(String path)
        {
            Scene scene;
            using (var importer = new AssimpImporter())
            {
                importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
                scene = importer.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality);
            }

            return scene;
        }

        private Vector3 FromVector(Vector3D vector)
        {
            return new Vector3
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
        }
        private Matrix4 FromMatrix(Matrix4x4 mat)
        {
            var m = new Matrix4();
            m.M11 = mat.A1;
            m.M12 = mat.A2;
            m.M13 = mat.A3;
            m.M14 = mat.A4;
            m.M21 = mat.B1;
            m.M22 = mat.B2;
            m.M23 = mat.B3;
            m.M24 = mat.B4;
            m.M31 = mat.C1;
            m.M32 = mat.C2;
            m.M33 = mat.C3;
            m.M34 = mat.C4;
            m.M41 = mat.D1;
            m.M42 = mat.D2;
            m.M43 = mat.D3;
            m.M44 = mat.D4;

            return m;
        }

        #endregion
    }

    public enum PoseType
    {
        Pose,
        Animation
    }
}
