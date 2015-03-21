using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using SharpDX.Toolkit.Input;

using System.Diagnostics;

namespace Project1
{
    class Camera
    {
        public Vector3 cameraPosition;
        public float horizontalAngle;
        public float verticalAngle;
        public float FOV;
        public float moveSpeed;
        public float mouseSpeed;
        public float xRes;
        public float yRes;
        private bool debugOn;
        private BoundingSphere bounds;

        public Camera(Vector3 position, float xr, float yr, float fov, bool debug)
        {
            cameraPosition = position;
            horizontalAngle = 0.0f;
            verticalAngle = -(float)Math.PI / 4.0f; //start looking down a little
            FOV = fov;
            moveSpeed = 0.1f;
            mouseSpeed = 0.00005f;
            xRes = xr;
            yRes = yr;
            debugOn = debug;
            bounds = new BoundingSphere(position, 1.0f);
        }

        // Takes changes in the mouse position and camera position and returns new view and projection matrices to apply to models
        public Tuple<Matrix, Matrix> Update(GameTime delta, float mouseX, float mouseY, KeyboardState kb, Vector3 translation)
        {
            this.horizontalAngle -= mouseSpeed * delta.ElapsedGameTime.Milliseconds * (float)(xRes / 2 - mouseX);
            this.verticalAngle += mouseSpeed * delta.ElapsedGameTime.Milliseconds * (float)(yRes / 2 - mouseY);

            var dir = computeDirection();
            var rightAxis = computeRAxis();
            var upAxis = computeUpAxis(rightAxis, dir);

            cameraPosition += translation;

            //update position here using keyboard values
            if (kb.IsKeyDown(Keys.W))
            {
                cameraPosition += dir * delta.ElapsedGameTime.Milliseconds * moveSpeed;
            }
            if (kb.IsKeyDown(Keys.S))
            {
                cameraPosition -= dir * delta.ElapsedGameTime.Milliseconds * moveSpeed;
            }
            if (kb.IsKeyDown(Keys.A))
            {
                cameraPosition -= rightAxis * delta.ElapsedGameTime.Milliseconds * moveSpeed;
            }
            if (kb.IsKeyDown(Keys.D))
            {
                cameraPosition += rightAxis * delta.ElapsedGameTime.Milliseconds * moveSpeed;
            }
            if (kb.IsKeyDown(Keys.Space))
            {
                cameraPosition += upAxis * delta.ElapsedGameTime.Milliseconds * moveSpeed;
            }
            if (kb.IsKeyDown(Keys.Control))
            {
                cameraPosition -= upAxis * delta.ElapsedGameTime.Milliseconds * moveSpeed;
            }

            var viewMatrix = Matrix.LookAtLH(cameraPosition, cameraPosition + dir, upAxis);
            var projectionMatrix = Matrix.PerspectiveFovLH(FOV, xRes / yRes, 0.1f, 300.0f);

            bounds.Center = cameraPosition;

            return Tuple.Create(viewMatrix, projectionMatrix);
        }

        // Computes the direction the camera is facing
        private Vector3 computeDirection()
        {
            return new Vector3(
                (float)(Math.Cos(verticalAngle) * Math.Sin(horizontalAngle)),
                (float)(Math.Sin(verticalAngle)),
                (float)(Math.Cos(verticalAngle) * Math.Cos(horizontalAngle))
            );
        }

        // Computes the horizontal axis of the camera
        private Vector3 computeRAxis()
        {
            return new Vector3(
                (float)(Math.Sin(horizontalAngle + Math.PI / 2.0f)),
                0.0f,
                (float)(Math.Cos(horizontalAngle + Math.PI / 2.0f))
            );
        }

        // Computes the upwards vector of the camera, which is perpendicular to the right and direction axes
        private Vector3 computeUpAxis(Vector3 r, Vector3 l)
        {
            return -1 * Vector3.Cross(r, l);
        }
    }
}
