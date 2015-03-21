// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// DONE

using SharpDX;
using SharpDX.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX.Direct3D11;

namespace Project1
{
    // Use this namespace here in case we need to use Direct3D11 namespace as well, as this
    // namespace will override the Direct3D11.
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;

    public class Project1Game : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;
        private Terrain model; // The landscape
        private Water waterPlane; // The water
        private KeyboardManager keyboardManager;
        private KeyboardState keyboardState;
        private MouseManager mouseManager;
        private MouseState mouseState;
        private Camera firstPersonView; // The camera
        private float xResolution; // X resolution in pixels
        private float yResolution; // Y resolution in pixels
        private bool debugOn; // Debug mode toggle
        private int sideLength; // Length of the square landscape, in number of vertices
        private float sunAngle; // The angle the sun makes in the world relative to the landscape
        private float daySpeed; // The speed that the sun sets and rises
        private Vector3 previousTranslation; // Saves the amount the world was moved in the previous frame in order to subtract 
        private float sunIntensity; //

        /// <summary>
        /// Initializes a new instance of the <see cref="Project1Game" /> class.
        /// </summary>
        public Project1Game()
        {
            // Creates a graphics manager. This is mandatory.
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            // Create the keyboard and mouse managers
            keyboardManager = new KeyboardManager(this);
            mouseManager = new MouseManager(this);

            // Set the debug mode
            debugOn = false;
        }

        protected override void LoadContent()
        {
            // Calculate the side length as 2 raised to a power, plus 1
            var magnitude = 6;
            sideLength = (int)Math.Pow(2, magnitude) + 1;
            
            // Set the default lighting attributes
            sunAngle = (float)Math.PI*1.5f; // Start the sun on the edge of the landscape
            sunIntensity = 0.5f;
            previousTranslation = new Vector3(0,0,0); // Nothing has moved yet
            daySpeed = 0.02f; // Adjust this to set the length of a "day"
            var randomRange = 30f; // The range of values to increase/decrease the terrain in the generation algo
 
            // Create the landscape and water plane
            model = new Terrain(this, sideLength, 0.0f, debugOn, randomRange);
            waterPlane = new Water(this,sideLength, debugOn, randomRange);
            
            // Set the x and y ratios now that we have a graphics device
            xResolution = (float)this.GraphicsDevice.BackBuffer.Width;
            yResolution = (float)this.GraphicsDevice.BackBuffer.Height;

            // Create a camera to view through
            firstPersonView = new Camera(new Vector3(0f, 30f, -40f), this.xResolution, this.yResolution, (float)Math.PI / 4.0f, debugOn);

            base.LoadContent();
        }

        protected override void Initialize()
        {
            Window.Title = "Project 1";

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            // Grab the current status of the keyboard and mouse
            keyboardState = keyboardManager.GetState();
            mouseState = mouseManager.GetState();

            // Save the mouse positions
            var mouseX = mouseState.X;
            var mouseY = mouseState.Y;

            // Reset mouse to centre of window
            mouseManager.SetPosition(new Vector2(1 / 2.0f, 1 / 2.0f));

            // Get the angle in the sky the sun is sitting at
            sunAngle += (float)(gameTime.ElapsedGameTime.Milliseconds * 0.005 * daySpeed * Math.PI);
            if (sunAngle > 2*Math.PI) sunAngle -= (float)(2*Math.PI); // make sure 360 degrees goes back to 0

            // Calculate the movement of the world around the origin (the sun)
            var expansionFactor = 1; // Factor to increase the radius by (to increase the distance from the sun)
            var newTranslation = new Vector3((float)Math.Cos(sunAngle) * expansionFactor*2 * sideLength / 2, (float)Math.Sin(sunAngle) * expansionFactor * sideLength / 2, 0); // Gets the amount to move the landscape by (in a circle around the sun)
            var translation = newTranslation - previousTranslation; // Get the amount to translate the landscape by, by subtracting the amount it has moved already, by the new amount (gives a small amount)
            previousTranslation = newTranslation; // Save for the next update

            // Get the projection and view matrices for the camera position
            var matrices = firstPersonView.Update(gameTime, mouseX * xResolution, mouseY * yResolution, keyboardState, translation);
            var viewMatrix = matrices.Item1;
            var projMatrix = matrices.Item2;
            var worldMatrix = Matrix.Identity;

            // Calculate the sun's attributes
            var sunPosition = Vector3.Normalize(-newTranslation);
            var sunColour = new Vector3(sunIntensity*1f,sunIntensity*0.8431373f,sunIntensity*0.4f);
            var sunColourSpec = new Vector3(sunIntensity * 1f, sunIntensity * 1f, sunIntensity*0.75f);
            var sunColourAmb = new Vector3(0.1f,0.1f,0.1f);

            // Update the models with the new positions
            model.Update(viewMatrix, projMatrix, worldMatrix, sunPosition, sunColour, sunColourSpec, sunColourAmb, translation);
            waterPlane.Update(viewMatrix, projMatrix, worldMatrix, sunPosition, sunColour, sunColourSpec, sunColourAmb, translation);

            // Turn wireframes and other debug features on or off by pressing X
            if (keyboardState.IsKeyPressed(Keys.X))
            {
                if (debugOn) debugOn = false;
                else debugOn = true;
            }
            // Quit with Esc
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
                this.Dispose();
            }

            // Handle base.Update
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.Clear(Color.Gray);

            if (debugOn)
            {
                // Drawing wireframes and removing culling
                var wireframe = RasterizerStateDescription.Default();
                wireframe.FillMode = FillMode.Wireframe;
                wireframe.CullMode = CullMode.None;
                var wf = RasterizerState.New(this.GraphicsDevice, wireframe);
                GraphicsDevice.SetRasterizerState(wf);
            }

            // Draw the terrain and water
            model.Draw(gameTime);
            waterPlane.Draw(gameTime);
            
            // Handle base.Draw
            base.Draw(gameTime);
        }
    }
}
