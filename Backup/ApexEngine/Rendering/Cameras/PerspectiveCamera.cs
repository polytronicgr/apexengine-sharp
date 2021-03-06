﻿using ApexEngine.Math;

namespace ApexEngine.Rendering.Cameras
{
    public class PerspectiveCamera : Camera
    {
        protected Quaternion rotation = new Quaternion();
        protected float fov = 45f, yaw, roll, pitch;
        private Vector3f tmp = new Vector3f();

        public PerspectiveCamera()
            : base()
        {
        }

        public PerspectiveCamera(float fov, int width, int height)
            : base(width, height)
        {
            this.fov = fov;
        }

        public float FieldOfView
        {
            get { return fov; }
            set { fov = value; }
        }

        public override void UpdateMatrix()
        {
            tmp.Set(translation);
            tmp.AddStore(direction);
            
            rotation.SetToLookAt(direction, up);
            viewMatrix.SetToLookAt(translation, tmp, up);
            projMatrix.SetToProjection(fov, width, height, 0.05f, far);
            viewProjMatrix.Set(projMatrix);
            viewProjMatrix.MultiplyStore(viewMatrix);
            invViewProjMatrix.Set(viewProjMatrix);
            invViewProjMatrix.InvertStore();

            yaw = rotation.GetYaw();
            roll = rotation.GetRoll();
            pitch = rotation.GetPitch();
        }

        public override void UpdateCamera()
        {
        }

        public float GetYaw()
        {
            return yaw;
        }

        public float GetRoll()
        {
            return roll;
        }

        public float GetPitch()
        {
            return pitch;
        }
    }
}