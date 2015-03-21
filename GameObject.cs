using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project1
{
    using SharpDX.Toolkit.Graphics;
    abstract public class GameObject
    {
        public BasicEffect basicEffect;
        public VertexInputLayout inputLayout;
        public Game game;

        public abstract void Update(Matrix vMatrix, Matrix pMatrix, Matrix wMatrix, Vector3 l1_dir, Vector3 l1_colour, Vector3 l1_colour_spec, Vector3 l1_colour_amb, Vector3 movement);
        public abstract void Draw(GameTime gametime);
    }
}
