using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

//include GLM library
using GlmNet;

using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Graphics
{
    class Renderer
    {
        Shader sh;

        uint rocketBodyBufferID;
        uint rocketNoseBufferID;
        uint rocketFinsBufferID;
        uint rocketBaseBufferID;
        uint rocketWindowsBufferID;
        uint xyzAxesBufferID;

        mat4 ModelMatrix;
        mat4 ViewMatrix;
        mat4 ProjectionMatrix;

        int ShaderModelMatrixID;
        int ShaderViewMatrixID;
        int ShaderProjectionMatrixID;

        const float rotationSpeed = 0.5f;
        float rotationAngle = 0;

        public float translationX = 0,
                     translationY = 0,
                     translationZ = 0;

        Stopwatch timer = Stopwatch.StartNew();

        vec3 rocketCenter = new vec3(30, 20, 0);

        const float moveSpeed = 10.0f;

        Texture finTexture;
        public void Initialize()
        {
            string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            sh = new Shader(projectPath + "\\Shaders\\SimpleVertexShader.vertexshader", projectPath + "\\Shaders\\SimpleFragmentShader.fragmentshader");
            Gl.glClearColor(0, 0, 0.4f, 1);

            finTexture = new Texture(projectPath + "\\Textures\\images.jpeg", 1);

            float centerX = 30.0f;
            float centerY = 20.0f;
            float centerZ = 0.0f;

            // rocket body
            List<float> rocketBodyVertices = new List<float>();
            float radius = 5.0f;
            float height = 20.0f;
            int segments = 20;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(2 * Math.PI * i / segments);
                float angle2 = (float)(2 * Math.PI * (i + 1) / segments);

                float x1 = (float)(radius * Math.Cos(angle1));
                float z1 = (float)(radius * Math.Sin(angle1));
                float x2 = (float)(radius * Math.Cos(angle2));
                float z2 = (float)(radius * Math.Sin(angle2));

                rocketBodyVertices.AddRange(new float[] {
                    centerX, centerY, centerZ, 0.8f, 0.2f, 0.2f,  // Center
                    centerX + x1, centerY, centerZ + z1, 0.8f, 0.2f, 0.2f, // Point 1
                    centerX + x2, centerY, centerZ + z2, 0.8f, 0.2f, 0.2f  // Point 2
                });

                rocketBodyVertices.AddRange(new float[] {
                    centerX, centerY + height, centerZ, 0.8f, 0.2f, 0.2f,  // Center
                    centerX + x1, centerY + height, centerZ + z1, 0.8f, 0.2f, 0.2f, // Point 1
                    centerX + x2, centerY + height, centerZ + z2, 0.8f, 0.2f, 0.2f  // Point 2
                });

                rocketBodyVertices.AddRange(new float[] {
                    centerX + x1, centerY, centerZ + z1, 0.7f, 0.1f, 0.1f,
                    centerX + x2, centerY, centerZ + z2, 0.7f, 0.1f, 0.1f,
                    centerX + x1, centerY + height, centerZ + z1, 0.9f, 0.3f, 0.3f,

                    centerX + x1, centerY + height, centerZ + z1, 0.9f, 0.3f, 0.3f,
                    centerX + x2, centerY, centerZ + z2, 0.7f, 0.1f, 0.1f,
                    centerX + x2, centerY + height, centerZ + z2, 0.9f, 0.3f, 0.3f
                });
            }

            List<float> rocketNoseVertices = new List<float>();
            float noseHeight = 10.0f;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(2 * Math.PI * i / segments);
                float angle2 = (float)(2 * Math.PI * (i + 1) / segments);

                float x1 = (float)(radius * Math.Cos(angle1));
                float z1 = (float)(radius * Math.Sin(angle1));
                float x2 = (float)(radius * Math.Cos(angle2));
                float z2 = (float)(radius * Math.Sin(angle2));

                rocketNoseVertices.AddRange(new float[] {
                    centerX, centerY + height + noseHeight, centerZ, 1.0f, 0.5f, 1.0f,  // Top point
                    centerX + x1, centerY + height, centerZ + z1, 0f, 0.4f, 1.0f, // Base point 1
                    centerX + x2, centerY + height, centerZ + z2, 0f, 0.4f, 1.0f  // Base point 2
                });
            }

            List<float> rocketFinsVertices = new List<float>();
            float finWidth = 2.0f;
            float finLength = 7.0f;
            float finHeight = 8.0f;
            // rocket fins
            for (int i = 0; i < 4; i++)
            {
                float angle = (float)(Math.PI / 2 * i);
                float xPos = (float)(radius * Math.Cos(angle));
                float zPos = (float)(radius * Math.Sin(angle));
                float xDir = (float)(Math.Cos(angle));
                float zDir = (float)(Math.Sin(angle));

                rocketFinsVertices.AddRange(new float[] {
                    centerX + xPos, centerY, centerZ + zPos,              0.0f, 0.0f, 0.0f,  0.0f, 1.0f,  // Bottom-left corner
                    centerX + xPos + finWidth * zDir, centerY, centerZ + zPos - finWidth * xDir, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,  // Bottom-right
                    centerX + xPos + finWidth * zDir, centerY + finHeight, centerZ + zPos - finWidth * xDir, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,  // Top-right
                    centerX + xPos, centerY + finHeight, centerZ + zPos,  0.0f, 0.0f, 0.0f,  0.0f, 0.0f   // Top-left
                });

                rocketFinsVertices.AddRange(new float[] {
                    centerX + xPos, centerY, centerZ + zPos,              0.0f, 0.0f, 0.0f,  0.0f, 1.0f,  // Bottom-left
                    centerX + xPos + finLength * xDir, centerY, centerZ + zPos + finLength * zDir, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,  // Bottom-right
                    centerX + xPos + finLength * xDir, centerY + finHeight, centerZ + zPos + finLength * zDir, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,  // Top-right
                    centerX + xPos, centerY + finHeight, centerZ + zPos,  0.0f, 0.0f, 0.0f,  0.0f, 0.0f   // Top-left
                });

                rocketFinsVertices.AddRange(new float[] {
                    centerX + xPos + finWidth * zDir, centerY, centerZ + zPos - finWidth * xDir, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,  // Bottom-left
                    centerX + xPos + finLength * xDir, centerY, centerZ + zPos + finLength * zDir, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,  // Bottom-right
                    centerX + xPos + finLength * xDir, centerY + finHeight, centerZ + zPos + finLength * zDir, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,  // Top-right
                    centerX + xPos + finWidth * zDir, centerY + finHeight, centerZ + zPos - finWidth * xDir, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f  // Top-left
                });
            }

            // Rocket base
            List<float> rocketBaseVertices = new List<float>();
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)(2 * Math.PI * i / segments);
                float angle2 = (float)(2 * Math.PI * (i + 1) / segments);

                float x1 = (float)(radius * Math.Cos(angle1));
                float z1 = (float)(radius * Math.Sin(angle1));
                float x2 = (float)(radius * Math.Cos(angle2));
                float z2 = (float)(radius * Math.Sin(angle2));

                rocketBaseVertices.AddRange(new float[] {
                    centerX, centerY, centerZ, 0.5f, 0.5f, 0.5f,  // Center
                    centerX + x1, centerY, centerZ + z1, 0.4f, 0.4f, 0.4f, // Point 1
                    centerX + x2, centerY, centerZ + z2, 0.4f, 0.4f, 0.4f  // Point 2
                });
            }

            // Rocket windows 
            List<float> rocketWindowsVertices = new List<float>();
            float windowSize = 1.5f;
            float windowHeight = 1.5f;

            for (int i = 0; i < 4; i++)
            {
                float angle = (float)(Math.PI / 2 * i + Math.PI / 4);

                float xCenter = (float)(radius * Math.Cos(angle));
                float zCenter = (float)(radius * Math.Sin(angle));

                float xTan = (float)(-Math.Sin(angle));
                float zTan = (float)(Math.Cos(angle));

                float x1 = xCenter + xTan * (windowSize / 2);
                float z1 = zCenter + zTan * (windowSize / 2);

                float x2 = xCenter - xTan * (windowSize / 2);
                float z2 = zCenter - zTan * (windowSize / 2);

                float x3 = x2;
                float z3 = z2;

                float x4 = x1;
                float z4 = z1;

                rocketWindowsVertices.AddRange(new float[] {
                    centerX + x1, centerY + height/2 - windowHeight/2, centerZ + z1, 1.0f, 1.0f, 0.8f,
                    centerX + x2, centerY + height/2 - windowHeight/2, centerZ + z2, 1.0f, 1.0f, 0.8f,
                    centerX + x3, centerY + height/2 + windowHeight/2, centerZ + z3, 1.0f, 1.0f, 0.8f,
                    centerX + x4, centerY + height/2 + windowHeight/2, centerZ + z4, 1.0f, 1.0f, 0.8f
                });
            }

            // XYZ axes
            float[] xyzAxesVertices = {
                //x - red
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                100.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                //y - green
                0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 100.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                //z - blue
                0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                0.0f, 0.0f, -100.0f, 0.0f, 0.0f, 1.0f,
            };
            // buffer
            rocketBodyBufferID = GPU.GenerateBuffer(rocketBodyVertices.ToArray());
            rocketNoseBufferID = GPU.GenerateBuffer(rocketNoseVertices.ToArray());
            rocketFinsBufferID = GPU.GenerateBuffer(rocketFinsVertices.ToArray());
            rocketBaseBufferID = GPU.GenerateBuffer(rocketBaseVertices.ToArray());
            rocketWindowsBufferID = GPU.GenerateBuffer(rocketWindowsVertices.ToArray());
            xyzAxesBufferID = GPU.GenerateBuffer(xyzAxesVertices);

            // Update rocket center for camera view
            rocketCenter = new vec3(centerX, centerY + height / 2, centerZ);

            ViewMatrix = glm.lookAt(
                new vec3(40, 40, 70),
                rocketCenter,
                new vec3(0, 1, 0)
            );


            ModelMatrix = new mat4(1);

            ProjectionMatrix = glm.perspective(45.0f, 4.0f / 3.0f, 0.1f, 200.0f);

            sh.UseShader();

            ShaderModelMatrixID = Gl.glGetUniformLocation(sh.ID, "modelMatrix");
            ShaderViewMatrixID = Gl.glGetUniformLocation(sh.ID, "viewMatrix");
            ShaderProjectionMatrixID = Gl.glGetUniformLocation(sh.ID, "projectionMatrix");

            Gl.glUniformMatrix4fv(ShaderViewMatrixID, 1, Gl.GL_FALSE, ViewMatrix.to_array());
            Gl.glUniformMatrix4fv(ShaderProjectionMatrixID, 1, Gl.GL_FALSE, ProjectionMatrix.to_array());

            timer.Start();
        }
        public void KeyPressTransformation(char key)
        {
            float speed = 5;

            if (key == 'd')
                translationX += speed;
            if (key == 'a')
                translationX -= speed;

            if (key == 'w')
                translationY += speed;
            if (key == 's')
                translationY -= speed;

            if (key == 'z')
                translationZ += speed;
            if (key == 'c')
                translationZ -= speed;
        }
        public void Draw()
        {
            sh.UseShader();
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            // XYZ axis
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, xyzAxesBufferID);

            Gl.glUniformMatrix4fv(ShaderModelMatrixID, 1, Gl.GL_FALSE, new mat4(1).to_array());

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_LINES, 0, 6);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
            

            Gl.glUniformMatrix4fv(ShaderModelMatrixID, 1, Gl.GL_FALSE, ModelMatrix.to_array());

            // Rocket Body
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, rocketBodyBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 20 * 12);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
            

            // Rocket Nose
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, rocketNoseBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 20 * 3);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
            

            // Rocket Fins

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, rocketFinsBufferID);
            Gl.glActiveTexture(Gl.GL_TEXTURE0);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)0);

            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glEnableVertexAttribArray(2);
            Gl.glVertexAttribPointer(2, 2, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)(6 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_QUADS, 0, 4 * 12);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
            Gl.glDisableVertexAttribArray(2);



            // Rocket Base
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, rocketBaseBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 20 * 3); 

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);

            // Rocket Windows
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, rocketWindowsBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_QUADS, 0, 4 * 4);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
        }

        public void Update()
        {
            timer.Stop();
            var deltaTime = timer.ElapsedMilliseconds / 1000.0f;
            rotationAngle += deltaTime * rotationSpeed;

            List<mat4> transformations = new List<mat4>();
            transformations.Add(glm.translate(new mat4(1), -1 * rocketCenter));
            transformations.Add(glm.rotate(rotationAngle, new vec3(0, 1, 0)));
            transformations.Add(glm.translate(new mat4(1), rocketCenter));

            transformations.Add(glm.translate(new mat4(1), new vec3(translationX, translationY, translationZ)));

            ModelMatrix = MathHelper.MultiplyMatrices(transformations);

            timer.Reset();
            timer.Start();
        }

        public void CleanUp()
        {
            sh.DestroyShader();
        }
    }
}