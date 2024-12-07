using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Common
{
    /// <summary>
    /// 着色器
    /// </summary>
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        
        public Shader(string vertPath, string fragPath)
        {
            
            var shaderSource = File.ReadAllText(vertPath);

            
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

           
            GL.ShaderSource(vertexShader, shaderSource);

          
            CompileShader(vertexShader);

          
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            
            Handle = GL.CreateProgram();

           
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

           
            LinkProgram(Handle);

            
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);


            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();

           
            for (var i = 0; i < numberOfUniforms; i++)
            {             
                var key = GL.GetActiveUniform(Handle, i, out _, out _);               
                var location = GL.GetUniformLocation(Handle, key);               
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            
            GL.CompileShader(shader);

            
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
               
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            
            GL.LinkProgram(program);

           
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

      
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

       
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }


        public Shader(string vert, string frag, int id)
        {
            var shaderSource = vert;

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);

            CompileShader(vertexShader);

            shaderSource = frag;

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

           
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

           
            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();

           
            for (var i = 0; i < numberOfUniforms; i++)
            {
                
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
              
                var location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);
            }
        }

        public Shader(string vert, string geometry, string frag, int id)
        {
            var shaderSource = vert;

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            shaderSource = geometry;
            var geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometryShader, shaderSource);
            CompileShader(geometryShader);

            shaderSource = frag;
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

            
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, geometryShader);
            GL.AttachShader(Handle, fragmentShader);

            
            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, geometryShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(geometryShader);


            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();


            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                var location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);

            }
        }

        public Shader(string shader, int name)
        {
            var shaderSource = shader;

            int geometryShader = 0;
            switch (name)
            {
                case 0:
                    geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                    break;
                case 1:
                    geometryShader = GL.CreateShader(ShaderType.VertexShader);
                    break;
                case 2:
                    geometryShader = GL.CreateShader(ShaderType.FragmentShader);
                    break;
                default:
                    geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                    break;
            }

            GL.ShaderSource(geometryShader, shaderSource);

            CompileShader(geometryShader);

            Handle = GL.CreateProgram();
           
            GL.AttachShader(Handle, geometryShader);
       
            LinkProgram(Handle);

            GL.DetachShader(Handle, geometryShader);

            GL.DeleteShader(geometryShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();
         
            for (var i = 0; i < numberOfUniforms; i++)
            {             
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
              
                var location = GL.GetUniformLocation(Handle, key);
             
                _uniformLocations.Add(key, location);
            }
        }


       
    }

}
