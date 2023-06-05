using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using System.Drawing;

namespace SharpDX9App {
    class GameWindow {
        private Form mWindow;
        private Microsoft.DirectX.Direct3D.Device mGraphicsDevice;
        private ulong mStartTime;
        private ulong mCurrTime;
        private ulong mPrevTime;

        private bool mStillIdle {
            get {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        protected Microsoft.DirectX.Direct3D.Device GraphicsDevice { get { return mGraphicsDevice; } }

        public GameWindow(string title) {
            mWindow = new Form();
            mWindow.Size = new Size(640, 480);
            mWindow.Text = title;
            mWindow.MinimizeBox = false;
            mWindow.MaximizeBox = false;
            mWindow.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        private void fInitDevice() {
            PresentParameters param = new PresentParameters();
            param.Windowed = true;
            param.SwapEffect = SwapEffect.Discard;
            param.BackBufferCount = 2;

            mGraphicsDevice = new Device(0, DeviceType.Hardware, mWindow, CreateFlags.HardwareVertexProcessing, param);

            LoadContent();
        }

        void Application_Idle(object sender, EventArgs e) {
            while (mStillIdle) {

                // Get delta time
                uint cTime = NativeMethods.GetTickCount();
                if (cTime != mPrevTime) {
                    mPrevTime = mCurrTime;
                    mCurrTime = cTime;
                }

                this.Update(((double)(mCurrTime - mPrevTime)) / 1000.0);
                this.Draw();

                GraphicsDevice.Present(mWindow);
            }
        }

        void mWindow_Shown(object sender, EventArgs e) {
            this.fInitDevice();
            mWindow.Focus();
        }


        public void Run() {
            mStartTime = NativeMethods.GetTickCount();
            
            mWindow.Shown += new EventHandler(mWindow_Shown);
            mWindow.Show();
            Application.Idle += new EventHandler(Application_Idle);
            Application.Run(mWindow);
        }

        public virtual void LoadContent() { }
        public virtual void Update(double deltaTime) { }
        public virtual void Draw() { }
    }
}
