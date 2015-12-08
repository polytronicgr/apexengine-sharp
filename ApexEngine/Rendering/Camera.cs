﻿using ApexEngine.Math;

namespace ApexEngine.Rendering
{
    public abstract class Camera
    {
        protected Vector3f translation = new Vector3f(0, 0, 0);
        protected Vector3f direction = new Vector3f(0, 0, 1);
        protected Vector3f up = new Vector3f(0, 1, 0);
        protected int width = 512, height = 512;
        protected float near = 0.0f, far = 150.0f;
        protected Matrix4f viewMatrix = new Matrix4f(), projMatrix = new Matrix4f(), viewProjMatrix = new Matrix4f(), invViewProjMatrix = new Matrix4f();
        private Vector3f tmpVec = new Vector3f();
        protected bool enabled = true;

        public Camera()
        {
        }

        public Camera(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public Matrix4f ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        public Matrix4f ProjectionMatrix
        {
            get { return projMatrix; }
            set { projMatrix = value; }
        }

        public Matrix4f ViewProjectionMatrix
        {
            get { return viewProjMatrix; }
            set { viewProjMatrix = value; }
        }

        public Matrix4f InverseViewProjectionMatrix
        {
            get { return invViewProjMatrix; }
            set { invViewProjMatrix = value; }
        }

        public Vector3f Unproject(Vector2f mouseXY)
        {
            return Unproject(mouseXY.x, mouseXY.y);
        }

        public Vector3f Unproject(float mouseX, float mouseY)
        {
            Vector3f vec = new Vector3f();

            vec.x = -2f * (mouseX / width);
            vec.y = 2f * (mouseY / height);
            vec.z = 0f;
            
            vec.MultiplyStore(projMatrix.Invert());
            vec.MultiplyStore(viewMatrix.Invert());

            return vec;
        }

        public Vector2f Project(Vector3f location)
        {
            Vector2f vec = new Vector2f();
            Vector3f vec3 = new Vector3f(location);
            System.Console.WriteLine(vec3);

            vec3.MultiplyStore(viewMatrix);
            vec3.MultiplyStore(projMatrix);
            

            vec.x = (vec3.x * width)/-2f;
            vec.y = (vec3.y * height)/2f;

            return vec;
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public Vector3f Translation
        {
            get
            {
                return translation;
            }
            set
            {
                translation.Set(value);
            }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public Vector3f Direction
        {
            get { return direction; }
            set { direction.Set(value); }
        }

        public Vector3f Up
        {
            get { return up; }
            set { up.Set(value); }
        }

        public float Near
        {
            get { return near; }
            set { near = value; }
        }

        public float Far
        {
            get { return far; }
            set { far = value; }
        }

        public void Rotate(Vector3f axis, float angle)
        {
            direction.RotateStore(axis, angle);
            direction.NormalizeStore();
        }

        public void LookAtDirection(Vector3f dir)
        {
            direction.Set(dir);
        }

        public void LookAt(Vector3f location)
        {
            direction.Set(location.Subtract(translation));
        }

        public void Update()
        {
            if (enabled)
            {
                UpdateCamera();
            }
            UpdateMatrix();
        }

        public abstract void UpdateMatrix();

        public abstract void UpdateCamera();
    }
}