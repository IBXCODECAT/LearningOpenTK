﻿using IBX_Engine.Exceptions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IBX_Engine.Graphics
{
    internal class ShaderProgram : IDisposable
    {
        private bool disposedValue = false;

        /// <summary>
        /// The handle of the shader program
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// The maximum number of vertex attributes for the vertex shader on the current hardware
        /// </summary>
        public static int MaxVertexAttributes { get { return GL.GetInteger(GetPName.MaxVertexAttribs); } }

        internal static ShaderProgram CURRENT_BOUND_PROGRAM { get; private set; }

        public ShaderProgram(string vertexPath, string fragmentPath)
        {
            // Read the shader source from the vertex shader file
            string VertexShaderSource = File.ReadAllText(vertexPath);

            // Create the vertex shader and pass in the source
            int VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            // Compile the vertex shader
            GL.CompileShader(VertexShader);

            // Check if the vertex shader compiled successfully
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int vertexCompileStatus);

            // If the vertex shader failed to compile, throw an exception
            string vertexShaderInfoLog = GL.GetShaderInfoLog(VertexShader);
            ShaderCompilationException vertexCompileException = new($"Error occurred whilst compiling Vertex Shader @ {vertexPath}:\n{vertexShaderInfoLog}", vertexPath, vertexShaderInfoLog);
            EngineExceptionManager.FailCatastrophically(vertexCompileException, vertexCompileStatus == 0);

            // Read the shader source from the fragment shader file
            string FragmentShaderSource = File.ReadAllText(fragmentPath);

            // Create the fragment shader and pass in the source
            int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compile the fragment shader
            GL.CompileShader(FragmentShader);

            // Check if the fragment shader compiled successfully
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out int fragmentCompileStatus);

            // If the fragment shader failed to compile, throw an exception
            string fragmentShaderInfoLog = GL.GetShaderInfoLog(FragmentShader);
            ShaderCompilationException fragmentCompileException = new($"Error occurred whilst compiling Fragment Shader @ {fragmentPath}:\n{fragmentShaderInfoLog}", fragmentPath, fragmentShaderInfoLog);
            EngineExceptionManager.FailCatastrophically(fragmentCompileException, fragmentCompileStatus == 0);

            // Create the shader program
            Handle = GL.CreateProgram();

            // Attach the shaders to the program
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            // Link the program
            GL.LinkProgram(Handle);

            // Check if the program linked successfully
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int programLinkStatus);

            // If the program failed to link, throw an exception
            string shaderProgramLinkInfoLog = GL.GetProgramInfoLog(Handle);
            ShaderCompilationException programLinkException = new($"Error occurred whilst linking Program({Handle}):\n{shaderProgramLinkInfoLog}", null, shaderProgramLinkInfoLog);
            EngineExceptionManager.FailCatastrophically(programLinkException, programLinkStatus == 0);

            // Detach the shaders from the program and delete them because they are no longer needed
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }

        /// <summary>
        /// Gets the location of a uniform in the shader program
        /// </summary>
        /// <returns>The location for the attribute specified</returns>
        public int GetAttributeLocation(string attribName)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return GL.GetAttribLocation(Handle, attribName);
        }

        /// <summary>
        /// Use the shader program for rendering
        /// </summary>
        public void Use()
        {
            // If the object has been disposed of, throw an exception
            ObjectDisposedException.ThrowIf(disposedValue, this);

            // Use the shader program
            GL.UseProgram(Handle);

            CURRENT_BOUND_PROGRAM = this;
        }

        public void SetInt(string name, int value)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            int location = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(location, true, ref matrix);
        }

        public void SetVector3(string name, Vector3 vector)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, vector);
        }

        /// <summary>
        /// Gets the location of a uniform in the shader program
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <returns>The uniform location</returns>
        public int GetUniformLocation(string name)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return GL.GetUniformLocation(Handle, name);
        }

        /// <summary>
        /// Called when the object is being disposed of
        /// </summary>
        protected virtual void DisposeProcedure()
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure the object is properly disposed of
        /// </summary>
        ~ShaderProgram()
        {
            if (!disposedValue)
            {
                Logger.LogWarning($"Resource not disposed properly. Call Dispose() to properly dispose of the resource.");
            }
        }

        /// <summary>
        /// Disposes of the object
        /// </summary>
        public void Dispose()
        {
            DisposeProcedure();
            GC.SuppressFinalize(this);
        }
    }
}
