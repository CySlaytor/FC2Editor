using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolCollection : ToolPaint
    {
        private ParamCollection m_paramCollection = new ParamCollection(null);

        public override string GetToolName() => Localizer.Localize("TOOL_COLLECTIONS");
        public override Image GetToolImage() => Resources.Collection;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_paramCollection;
        }

        public override string GetContextHelp()
        {
            return Localizer.LocalizeCommon("HELP_TOOL_COLLECTION") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + Localizer.Localize("HELP_TOOL_COLLECTION_ERASE") + "\r\n\r\n" + GetShortcutContextHelp();
        }

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            int collectionId;
            if ((Control.ModifierKeys & Keys.Control) != Keys.None)
            {
                collectionId = CollectionManager.EmptyCollectionId;
            }
            else
            {
                collectionId = m_paramCollection.Value;
                if (collectionId == -1)
                    return;

                CollectionInventory.Entry entry = CollectionManager.GetCollectionEntryFromId(collectionId);
                if (!entry.IsValid)
                    return;
            }
            CollectionManipulator.Paint(pos, collectionId, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            CollectionManipulator.Paint_End();
        }

        public override void OnEditorEvent(uint eventType, IntPtr eventPtr)
        {
            if (eventType == EditorEventUndo.TypeId)
            {
                m_paramCollection.UpdateUIControls();
            }
        }
    }
}