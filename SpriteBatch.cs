using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using System.Resources;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX9App {
    struct SpriteVtx {
        public Vector4 Position;
        public int Color;
        public Vector2 UV;
    }

    class SpriteBatch {
        #region Private Variables
        private Device graphicsDevice;
        private VertexBuffer buffer;
        private PixelShader pixelshader;
        private VertexShader vertexshader;
        private System.Resources.ResourceManager res;

        private ushort idx;
        private Texture currTexture;
        private GraphicsStream writeTarget;

        private int SizeOfVtx { get { return Marshal.SizeOf(typeof(SpriteVtx)); } }
        private int SizeOfData;
        #endregion


        #region Private Functions
        private void fPushVerts(Vector2 vtx, Vector2 uv, Color color) {
            SpriteVtx vtxout = new SpriteVtx();
            vtxout.Color = color.ToArgb();
            vtxout.Position = new Vector4(vtx.X, vtx.Y, 0, 1);
            vtxout.UV = uv;

            idx++;
            writeTarget.Write(new SpriteVtx[] { vtxout });
        }
        #endregion

        #region Constructors
        public SpriteBatch(Device graphicsDevice) {
            this.graphicsDevice = graphicsDevice;

            this.SizeOfData = SizeOfVtx * 6 * 10000;
            this.buffer = new VertexBuffer(
                graphicsDevice,
                SizeOfData,
                Usage.Dynamic,
                VertexFormats.Position | VertexFormats.Specular | VertexFormats.Texture0,
                Pool.Default);


            byte[] vtxdata = (byte[])SharpDX9App.Resources.ResourceManager.GetObject("vtxbatch");
            byte[] frgdata = (byte[])SharpDX9App.Resources.ResourceManager.GetObject("frgbatch");

            unsafe {

                fixed (byte* vtxptr = vtxdata) {
                    this.vertexshader = new VertexShader(graphicsDevice, new GraphicsStream(
                            vtxptr, vtxdata.Length, true, false
                        )
                    );
                }

                fixed (byte* frgptr = frgdata) {
                    this.pixelshader = new PixelShader(graphicsDevice, new GraphicsStream(
                            frgptr, frgdata.Length, true, false
                        )
                    );
                }
            }

        }
        #endregion


        #region Public Methods

        /// <summary>
        /// Begins a sprite batcher pass
        /// </summary>
        public void Begin() {
            graphicsDevice.BeginScene();
            writeTarget = buffer.Lock(0, SizeOfVtx * idx, LockFlags.None);
        }

        public void Draw(Texture texture, Rectangle area, Rectangle uv, Color color) {

            // Handle flushing of batch if texture changes.
            if (currTexture != texture) {
                if (currTexture == null) {
                    currTexture = texture;
                } else {
                    currTexture = texture;
                    this.Flush(true);
                }
            }

            // Triangle 1
            fPushVerts(new Vector2(area.Left, area.Top), new Vector2(uv.Left, uv.Top), color);
            fPushVerts(new Vector2(area.Right, area.Top), new Vector2(uv.Right, uv.Top), color);
            fPushVerts(new Vector2(area.Left, area.Bottom), new Vector2(uv.Left, uv.Top), color);

            // Triangles 2
            fPushVerts(new Vector2(area.Left, area.Bottom), new Vector2(uv.Left, uv.Bottom), color);
            fPushVerts(new Vector2(area.Right, area.Top), new Vector2(uv.Right, uv.Top), color);
            fPushVerts(new Vector2(area.Right, area.Bottom), new Vector2(uv.Right, uv.Bottom), color);

            if (idx >= 6 * 10000) this.Flush();
        }

        /// <summary>
        /// Ends a spritebatcher pass
        /// </summary>
        public void End() {
            this.Flush();
            graphicsDevice.EndScene();
        }

        public void Flush() {
            this.Flush(false);
        }

        public void Flush(bool reuse) {
            buffer.Unlock();

            // Draw
            graphicsDevice.SetStreamSource(0, buffer, 0, SizeOfVtx);
            graphicsDevice.SetTexture(0, currTexture);
            graphicsDevice.PixelShader = pixelshader;
            graphicsDevice.VertexShader = vertexshader;
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, idx/3);
            graphicsDevice.SetTexture(0, null);

            idx = 0;
            if (reuse) writeTarget = buffer.Lock(0, SizeOfVtx * idx, LockFlags.None);
        }

        #endregion
    }
}
