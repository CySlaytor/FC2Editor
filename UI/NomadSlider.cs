using System.Windows.Forms;
using FC2Editor.Core;

namespace FC2Editor.UI
{
    internal class NomadSlider : TrackBar
    {
        private Win32.Rect m_channelRect = default(Win32.Rect);
        private bool m_dragging;

        public NomadSlider()
        {
            SetStyle(ControlStyles.UserMouse, value: true);
        }

        private void UpdateSliderFromMouse(MouseEventArgs e)
        {
            if (e.X < m_channelRect.left)
            {
                base.Value = base.Minimum;
            }
            else if (e.X >= m_channelRect.right)
            {
                base.Value = base.Maximum;
            }
            else
            {
                base.Value = base.Minimum + (e.X - m_channelRect.left) * (base.Maximum - base.Minimum) / (m_channelRect.right - m_channelRect.left);
            }
            OnScroll(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Win32.SendMessage(base.Handle, 1050, 0, ref m_channelRect); // TBM_GETCHANNELRECT
            UpdateSliderFromMouse(e);
            m_dragging = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (m_dragging)
            {
                UpdateSliderFromMouse(e);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            m_dragging = false;
            base.OnMouseUp(e);
        }
    }
}