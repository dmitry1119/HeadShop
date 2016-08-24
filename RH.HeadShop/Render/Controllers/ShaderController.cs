using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using RH.HeadShop.IO;

namespace RH.HeadShop.Render.Controllers
{
    public class ShaderController
    {
        #region Var

        private readonly int vertex;
        private readonly int fragment;
        private readonly int shader;
        private readonly Dictionary<String, int> uniforms = new Dictionary<string, int>();

        #endregion

        public ShaderController(String vsFileName, String fsFileName)
        {
            var dir = Path.GetDirectoryName(Application.ExecutablePath);
            var fullVS = Path.Combine(dir, vsFileName);
            var fullFS = Path.Combine(dir, fsFileName);

            using (var vs = new StreamReader(fullVS))
            using (var fs = new StreamReader(fullFS))
                CreateShaders(vs.ReadToEnd(), fs.ReadToEnd(),
                    out vertex, out fragment,
                    out shader);
        }

        #region Supported void's

        public bool SetUniformLocation(String name)
        {
            var location = GL.GetUniformLocation(shader, name);
            if (location == -1) // -1 = fail
            {
                ProgramCore.EchoToLog(String.Format("SetUniformLocation error: {0}", name), EchoMessageType.Error);
            }

            uniforms.Add(name, location);
            return true;
        }
        private void CreateShaders(string vs, string fs, out int vertexObject, out int fragmentObject, out int program)
        {
            int statusCode;
            string info;

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1)
                throw new ApplicationException(info);

            // Compile vertex shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1)
                throw new ApplicationException(info);

            program = GL.CreateProgram();
            GL.AttachShader(program, fragmentObject);
            GL.AttachShader(program, vertexObject);

            GL.LinkProgram(program);
        }

        public void Begin()
        {
            GL.UseProgram(shader);
        }
        public void End()
        {
            GL.UseProgram(0);
        }

        public bool UpdateUniform(String name, OpenTK.Matrix4[] matArray)
        {
            var data = new List<float>();
            foreach (var mat in matArray)
            {
                // Col 1
                data.Add(mat.M11);
                data.Add(mat.M21);
                data.Add(mat.M31);
                data.Add(mat.M41);

                // Col 2
                data.Add(mat.M12);
                data.Add(mat.M22);
                data.Add(mat.M32);
                data.Add(mat.M42);

                // Col 3
                data.Add(mat.M13);
                data.Add(mat.M23);
                data.Add(mat.M33);
                data.Add(mat.M43);
            }

            GL.Uniform4(uniforms[name], matArray.Count() * 3, data.ToArray());
            CheckError();

            return true;
        }
        public bool UpdateUniform(String name, int i)
        {
            GL.Uniform1(uniforms[name], i);

            CheckError();
            return true;
        }
        public bool UpdateUniform(String name, float f)
        {
            GL.Uniform1(uniforms[name], f);

            CheckError();
            return true;
        }
        public bool UpdateUniform(String name, OpenTK.Vector3 vec)
        {
            GL.Uniform3(uniforms[name], ref vec);

            CheckError();
            return true;
        }        
        public bool UpdateUniform(String name, OpenTK.Vector4 vec)
        {
            GL.Uniform4(uniforms[name], ref vec);

            CheckError();
            return true;
        }
        public bool UpdateUniform(String name, OpenTK.Matrix4 mat)
        {
            if (!uniforms.ContainsKey(name))
                return false;
            GL.UniformMatrix4(uniforms[name], false, ref mat);

            CheckError();
            return true;
        }

        private void CheckError()
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                ProgramCore.EchoToLog(error.ToString(), EchoMessageType.Error);
                throw new Exception(error.ToString());
            }
        }

        #endregion
    }
}
