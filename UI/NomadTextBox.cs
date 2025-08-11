using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FC2Editor.Core;

namespace FC2Editor.UI
{
    internal class NomadTextBox : Control
    {
        public struct Position
        {
            public int line;
            public int column;

            public Position(int line, int column)
            {
                this.line = line;
                this.column = column;
            }

            public static bool operator <(Position p1, Position p2)
            {
                if (p1.line < p2.line) return true;
                if (p1.line == p2.line) return p1.column < p2.column;
                return false;
            }
            public static bool operator >(Position p1, Position p2)
            {
                if (p1.line > p2.line) return true;
                if (p1.line == p2.line) return p1.column > p2.column;
                return false;
            }
            public static bool operator <=(Position p1, Position p2) => !(p1 > p2);
            public static bool operator >=(Position p1, Position p2) => !(p1 < p2);
            public static bool operator ==(Position p1, Position p2) => p1.line == p2.line && p1.column == p2.column;
            public static bool operator !=(Position p1, Position p2) => !(p1 == p2);

            public override bool Equals(object obj)
            {
                if (obj is Position) return this == (Position)obj;
                return base.Equals(obj);
            }
            public override int GetHashCode() => line.GetHashCode() ^ column.GetHashCode();
        }

        protected struct Selection
        {
            public Position start;
            public Position end;
            public bool IsEmpty => start == end;

            public Selection(Position pos)
            {
                start = pos;
                end = pos;
            }

            public Selection(Position start, Position end)
            {
                this.start = start;
                this.end = end;
            }

            public void Normalize()
            {
                if (start > end)
                {
                    Position temp = start;
                    start = end;
                    end = temp;
                }
            }

            public bool Contains(Position pos) => pos >= start && pos < end;
            public static bool operator ==(Selection s1, Selection s2) => s1.start == s2.start && s1.end == s2.end;
            public static bool operator !=(Selection s1, Selection s2) => !(s1 == s2);
            public override bool Equals(object obj)
            {
                if (obj is Selection) return this == (Selection)obj;
                return base.Equals(obj);
            }
            public override int GetHashCode() => start.GetHashCode() ^ end.GetHashCode();
        }

        protected class Line
        {
            public string line;
            public IDisposable tag;
            public Line(string line) { this.line = line; }
        }

        protected class Lines : List<Line>
        {
            public string Text
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < base.Count; i++)
                    {
                        stringBuilder.AppendLine(base[i].line);
                    }
                    return stringBuilder.ToString();
                }
            }

            public Lines() { Add(new Line("")); }
            public void ClearTags(int start, int num)
            {
                for (int i = start; i < start + num; i++)
                {
                    base[i].tag?.Dispose();
                    base[i].tag = null;
                }
            }
            public void ClearTags() { ClearTags(0, base.Count); }
        }

        protected abstract class UndoCommand
        {
            public abstract void ToggleState(NomadTextBox textBox);
        }

        protected class UndoTextCommand : UndoCommand
        {
            private Selection m_selection;
            private string m_text;
            private bool m_insert;

            public UndoTextCommand(Selection selection, string text, bool insert)
            {
                m_selection = selection;
                m_text = text;
                m_insert = insert;
            }

            public override void ToggleState(NomadTextBox textBox)
            {
                if (m_insert)
                {
                    textBox.DeleteSelection(m_selection, false);
                }
                else
                {
                    textBox.Paste(m_text, m_selection.start, false);
                }
                m_insert = !m_insert;
            }
        }

        protected class UndoIndentCommand : UndoCommand
        {
            protected Selection selection;
            protected bool indent;

            public UndoIndentCommand(Selection selection, bool indent)
            {
                this.selection = selection;
                this.indent = indent;
            }

            public override void ToggleState(NomadTextBox textBox)
            {
                textBox.m_selection = selection;
                textBox.IndentSelection(!indent, false);
                selection = textBox.m_selection;
                indent = !indent;
            }
        }

        protected class UndoEntry
        {
            protected List<UndoCommand> m_undoList = new List<UndoCommand>();
            private bool m_undo = true;
            protected Position m_oldCaret;
            protected Selection m_oldSelection;
            protected Position m_newCaret;
            protected Selection m_newSelection;

            public void Start(NomadTextBox textBox)
            {
                m_oldCaret = textBox.m_caretPosition;
                m_oldSelection = textBox.m_selection;
            }

            public bool Stop(NomadTextBox textBox)
            {
                m_newCaret = textBox.m_caretPosition;
                m_newSelection = textBox.m_selection;
                return m_undoList.Count > 0;
            }

            public void AddCommand(UndoCommand cmd)
            {
                m_undoList.Add(cmd);
            }

            public void ToggleState(NomadTextBox textBox)
            {
                if (m_undo)
                {
                    for (int i = m_undoList.Count - 1; i >= 0; i--)
                    {
                        m_undoList[i].ToggleState(textBox);
                    }
                }
                else
                {
                    for (int i = 0; i < m_undoList.Count; i++)
                    {
                        m_undoList[i].ToggleState(textBox);
                    }
                }

                textBox.SetCaretPosition(m_oldCaret, true);
                textBox.SetSelection(m_oldSelection);

                Position oldCaret = m_oldCaret;
                m_oldCaret = m_newCaret;
                m_newCaret = oldCaret;

                Selection oldSelection = m_oldSelection;
                m_oldSelection = m_newSelection;
                m_newSelection = oldSelection;

                m_undo = !m_undo;
            }
        }

        protected Lines m_lines = new Lines();
        protected Position m_caretPosition;
        protected Position m_anchorPosition;
        protected Selection m_selection = default(Selection);
        protected Win32.TextMetric m_fontMetrics;
        protected Position m_origin;
        protected List<UndoEntry> m_undoEntries = new List<UndoEntry>();
        protected UndoEntry m_undoEntry;
        private int undoLevel;

        private static TextFormatFlags s_textFormatFlags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;
        private static uint s_drawTextFlags = 2272u;
        private static string[] s_lineSeparators = new string[] { "\r\n", "\r", "\n" };

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style |= 0x300000; // WS_HSCROLL | WS_VSCROLL
                createParams.ExStyle |= 0x200; // WS_EX_CLIENTEDGE
                return createParams;
            }
        }

        public Position CaretPosition
        {
            get { return m_caretPosition; }
            set { SetCaretPosition(value, true); }
        }

        protected int LineHeight => m_fontMetrics.tmHeight + 1;
        protected int CharWidth => m_fontMetrics.tmAveCharWidth;
        protected int NumVisibleLines => base.ClientRectangle.Height > 0 ? base.ClientRectangle.Height / LineHeight : 0;
        protected int NumVisibleChars => base.ClientRectangle.Width > 0 ? base.ClientRectangle.Width / CharWidth : 0;
        protected int LineStartPixel => -m_origin.column * CharWidth + LeftMarginWidth;
        protected int LeftMarginWidth => 16;

        public override string Text
        {
            get { return m_lines.Text; }
            set
            {
                m_lines = new Lines();
                m_undoEntries.Clear();
                Paste(value, false);
                SetCaretPosition(default(Position), true);
            }
        }

        public event EventHandler CaretPositionChanged;

        public NomadTextBox()
        {
            BackColor = SystemColors.Window;
            Cursor = Cursors.IBeam;
            Font = new Font("Lucida Console", 10f);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Win32.CreateCaret(base.Handle, IntPtr.Zero, 1, LineHeight);
            Win32.ShowCaret(base.Handle);
            SetCaretPosition(m_caretPosition, false);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Win32.HideCaret(base.Handle);
            Win32.DestroyCaret();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            using (Graphics g = this.CreateGraphics())
            {
                IntPtr hdc = g.GetHdc();
                IntPtr hFont = this.Font.ToHfont();
                IntPtr oldFont = Win32.SelectObject(hdc, hFont);
                Win32.GetTextMetrics(hdc, out m_fontMetrics);
                Win32.SelectObject(hdc, oldFont);
                Win32.DeleteObject(hFont);
                g.ReleaseHdc(hdc);
            }
            base.OnFontChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateScrollbars();
            base.OnSizeChanged(e);
        }

        protected int DrawTextRaw(Graphics g, IntPtr hFont, string s, int x, int y, bool draw, Color color, Color backColor)
        {
            IntPtr hdc = g.GetHdc();
            IntPtr oldFont = Win32.SelectObject(hdc, hFont);
            Win32.Rect lprc = new Win32.Rect(x, y, 0, LineHeight);
            Win32.DrawTextParams drawTextParams = new Win32.DrawTextParams { iTabLength = 4 };
            Win32.DrawTextEx(hdc, s, s.Length, ref lprc, s_drawTextFlags | 0x400, drawTextParams);
            int result = lprc.right - lprc.left;
            lprc.right = x + result; // ensure the rectangle width is correct for filling
            lprc.bottom = y + LineHeight;
            if (draw)
            {
                IntPtr hbr = Win32.CreateSolidBrush(ColorTranslator.ToWin32(backColor));
                Win32.FillRect(hdc, ref lprc, hbr);
                Win32.DeleteObject(hbr);
                Win32.SetTextColor(hdc, ColorTranslator.ToWin32(color));
                Win32.SetBkColor(hdc, ColorTranslator.ToWin32(backColor));
                Win32.DrawTextEx(hdc, s, s.Length, ref lprc, s_drawTextFlags, drawTextParams);
            }
            Win32.SelectObject(hdc, oldFont);
            g.ReleaseHdc();
            return result;
        }

        protected int DrawText(Graphics g, IntPtr hFont, Position position, int x, int y, string s, Color color, bool selected)
        {
            int drawnWidth = 0;
            if (!m_selection.IsEmpty)
            {
                if (!selected && m_selection.start.line == position.line && m_selection.start.column >= position.column && m_selection.start.column < position.column + s.Length)
                {
                    string preSelection = s.Substring(0, m_selection.start.column - position.column);
                    if (preSelection.Length > 0)
                    {
                        drawnWidth = DrawText(g, hFont, position, x, y, preSelection, color, false);
                        return drawnWidth + DrawText(g, hFont, new Position(position.line, position.column + preSelection.Length), x + drawnWidth, y, s.Substring(m_selection.start.column - position.column), color, true);
                    }
                    selected = true;
                }
                if (selected && m_selection.end.line == position.line && m_selection.end.column > position.column && m_selection.end.column <= position.column + s.Length)
                {
                    string inSelection = s.Substring(0, m_selection.end.column - position.column);
                    if (inSelection.Length > 0)
                    {
                        drawnWidth = DrawText(g, hFont, position, x, y, inSelection, color, true);
                        return drawnWidth + DrawText(g, hFont, new Position(position.line, position.column + inSelection.Length), x + drawnWidth, y, s.Substring(m_selection.end.column - position.column), color, false);
                    }
                    selected = false;
                }
            }

            drawnWidth = DrawTextRaw(g, hFont, s, x, y, true, selected ? SystemColors.HighlightText : color, selected ? SystemColors.Highlight : BackColor);
            if (selected && m_selection.end.line > position.line)
            {
                using (SolidBrush brush = new SolidBrush(SystemColors.Highlight))
                {
                    g.FillRectangle(brush, new Rectangle(x + drawnWidth, y, ClientRectangle.Width, LineHeight));
                }
            }
            return drawnWidth;
        }

        protected int DrawText(Graphics g, IntPtr hFont, Position position, int x, int y, string s, Color color)
        {
            bool selected = m_selection.Contains(position) || (m_selection.start.line < position.line && m_selection.end.line > position.line);
            return DrawText(g, hFont, position, x, y, s, color, selected);
        }

        protected virtual void DrawTextFormat(Graphics g, IntPtr hFont, Position position, int x, int y, string s)
        {
            DrawText(g, hFont, position, x, y, s, ForeColor);
        }

        protected virtual void OnPaintMargin(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(SystemColors.Control))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(0, 0, LeftMarginWidth, base.ClientRectangle.Height));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int numVisibleLines = NumVisibleLines + 1;
            int startLine = m_origin.line;
            int endLine = m_origin.line + numVisibleLines;
            if (endLine > m_lines.Count)
            {
                endLine = m_lines.Count;
            }
            IntPtr hFont = Font.ToHfont();
            int currentY = 0;
            for (int i = startLine; i < endLine; i++)
            {
                DrawTextFormat(e.Graphics, hFont, new Position(i, 0), LineStartPixel, currentY, m_lines[i].line);
                currentY += LineHeight;
            }
            Win32.DeleteObject(hFont);
            OnPaintMargin(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_GETDLGCODE:
                    m.Result = (IntPtr)4; // DLGC_WANTALLKEYS
                    break;
                case Win32.WM_VSCROLL:
                    OnScroll(true, m);
                    break;
                case Win32.WM_HSCROLL:
                    OnScroll(false, m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected void OnScroll(bool vertical, Message m)
        {
            int nBar = (vertical ? 1 : 0);
            Win32.ScrollInfo scrollInfo = new Win32.ScrollInfo { fMask = 31 };
            Win32.GetScrollInfo(base.Handle, nBar, scrollInfo);
            int pos = scrollInfo.nPos;
            switch (Win32.LoWord((int)m.WParam))
            {
                case 0: pos--; break; // SB_LINEUP
                case 1: pos++; break; // SB_LINEDOWN
                case 2: pos -= (int)scrollInfo.nPage; break; // SB_PAGEUP
                case 3: pos += (int)scrollInfo.nPage; break; // SB_PAGEDOWN
                case 4:
                case 5: pos = scrollInfo.nTrackPos; break; // SB_THUMBPOSITION, SB_THUMBTRACK
                case 6: pos = scrollInfo.nMin; break; // SB_TOP
                case 7: pos = scrollInfo.nMax; break; // SB_BOTTOM
            }

            Position origin = m_origin;
            if (vertical)
                origin.line = pos;
            else
                origin.column = pos;

            ScrollTo(origin);
        }

        protected Position GetPositionFromPoint(Point pt, out int x, out int y)
        {
            Position position = new Position
            {
                line = m_origin.line + pt.Y / LineHeight
            };
            ClipPositionLine(ref position);
            y = (position.line - m_origin.line) * LineHeight;
            string line = m_lines[position.line].line;
            x = LineStartPixel;
            using (Graphics dc = Graphics.FromHwnd(base.Handle))
            {
                position.column = 0;
                IntPtr hFont = Font.ToHfont();
                while (position.column < line.Length)
                {
                    string c = (line[position.column] == '\t') ? new string(' ', 4 - position.column % 4) : new string(line[position.column], 1);
                    int charWidth = DrawTextRaw(dc, hFont, c, 0, 0, false, Color.Empty, Color.Empty);

                    int newX = x + charWidth;
                    if (pt.X >= newX - charWidth / 2)
                    {
                        x = newX;
                        position.column++;
                        continue;
                    }
                    break;
                }
                Win32.DeleteObject(hFont);
            }
            ClipPositionColumn(ref position);
            return position;
        }

        protected void GetPointFromPosition(Position position, out int x, out int y)
        {
            string line = m_lines[position.line].line;
            using (Graphics g = Graphics.FromHwnd(base.Handle))
            {
                IntPtr hFont = Font.ToHfont();
                y = LineHeight * (position.line - m_origin.line);
                int width = DrawTextRaw(g, hFont, line.Substring(0, position.column), 0, 0, false, Color.Empty, Color.Empty);
                x = LineStartPixel + width;
                Win32.DeleteObject(hFont);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Position pos = GetPositionFromPoint(e.Location, out int x, out int y);
            Selection selection = ((Control.ModifierKeys & Keys.Shift) == 0) ? new Selection(pos) : new Selection(m_anchorPosition, pos);
            if ((Control.ModifierKeys & Keys.Control) != Keys.None)
            {
                ExpandSelectionWords(ref selection);
            }
            SetSelection(selection);
            if ((Control.ModifierKeys & Keys.Shift) == 0)
            {
                m_anchorPosition = pos;
            }
            SetCaretPosition(selection.end, x, y, false, true);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                Position pos = GetPositionFromPoint(e.Location, out int x, out int y);
                Selection selection = new Selection(m_anchorPosition, pos);
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    ExpandSelectionWords(ref selection);
                }
                SetSelection(selection);
                if (pos < m_anchorPosition)
                {
                    SetCaretPosition(m_selection.start, x, y, false, true);
                }
                else
                {
                    SetCaretPosition(m_selection.end, x, y, false, true);
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            Selection selection = new Selection(m_caretPosition);
            ExpandSelectionWords(ref selection);
            SetSelection(selection);
            m_anchorPosition = selection.start;
            SetCaretPosition(selection.end, false);
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int scrollLines = -e.Delta / SystemInformation.MouseWheelScrollDelta * SystemInformation.MouseWheelScrollLines;
            Position origin = m_origin;
            origin.line += scrollLines;
            ScrollTo(origin);
            base.OnMouseWheel(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar >= ' ')
            {
                BeginUndo();
                Paste(new string(e.KeyChar, 1));
                EndUndo();
            }
            base.OnKeyPress(e);
        }

        protected int SkipSpaceStart(string line, int index)
        {
            for (index--; index >= 0; index--)
            {
                if (line[index] != ' ' && line[index] != '\t')
                {
                    return index + 1;
                }
            }
            return 0;
        }

        protected int SkipSpaceEnd(string line, int index)
        {
            while (index < line.Length && (line[index] == ' ' || line[index] == '\t'))
            {
                index++;
            }
            return index;
        }

        protected int FindWordStart(string line, int index)
        {
            index--;
            int initialIndex = index;
            while (index >= 0)
            {
                char c = line[index];
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    if (index == initialIndex) index--;
                    return index + 1;
                }
                index--;
            }
            return 0;
        }

        protected int FindWordEnd(string line, int index)
        {
            int initialIndex = index;
            while (index < line.Length)
            {
                char c = line[index];
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    if (index == initialIndex) index++;
                    return index;
                }
                index++;
            }
            return line.Length;
        }

        protected void MoveTo(Position newPos)
        {
            ClipPosition(ref newPos);
            SetCaretPosition(newPos, (Control.ModifierKeys & Keys.Shift) == 0);
            Selection selection = new Selection(m_anchorPosition, newPos);
            SetSelection(selection);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    {
                        Position pos = m_caretPosition;
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            string line = m_lines[m_caretPosition.line].line;
                            int col = m_caretPosition.column;
                            col = SkipSpaceStart(line, col);
                            col = FindWordStart(line, col);
                            if (col == m_caretPosition.column) col--;
                            pos.column = col;
                        }
                        else if (pos.column > 0)
                        {
                            pos.column--;
                        }
                        else if (pos.line > 0)
                        {
                            pos.line--;
                            pos.column = m_lines[pos.line].line.Length;
                        }
                        MoveTo(pos);
                        break;
                    }
                case Keys.Right:
                    {
                        Position pos = m_caretPosition;
                        string line = m_lines[m_caretPosition.line].line;
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            int col = m_caretPosition.column;
                            col = FindWordEnd(line, col);
                            col = SkipSpaceEnd(line, col);
                            if (col == m_caretPosition.column) col++;
                            pos.column = col;
                        }
                        else if (pos.column < line.Length)
                        {
                            pos.column++;
                        }
                        else if (pos.line < m_lines.Count - 1)
                        {
                            pos.line++;
                            pos.column = 0;
                        }
                        MoveTo(pos);
                        break;
                    }
                case Keys.Up:
                    {
                        Position pos = m_caretPosition;
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            Position origin = m_origin;
                            origin.line--;
                            ScrollTo(origin);
                            if (m_caretPosition.line >= m_origin.line + NumVisibleLines) pos.line--;
                        }
                        else if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                        {
                            pos.line--;
                        }
                        else
                        {
                            pos.line = m_selection.start.line - 1;
                        }
                        MoveTo(pos);
                        break;
                    }
                case Keys.Down:
                    {
                        Position pos = m_caretPosition;
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            Position origin = m_origin;
                            origin.line++;
                            ScrollTo(origin);
                            if (m_caretPosition.line < m_origin.line) pos.line++;
                        }
                        else if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                        {
                            pos.line++;
                        }
                        else
                        {
                            pos.line = m_selection.end.line + 1;
                        }
                        MoveTo(pos);
                        break;
                    }
                case Keys.Home:
                    {
                        Position pos = m_caretPosition;
                        if ((e.Modifiers & Keys.Control) != Keys.None) pos.line = 0;
                        pos.column = 0;
                        MoveTo(pos);
                        break;
                    }
                case Keys.End:
                    {
                        Position pos = m_caretPosition;
                        if ((e.Modifiers & Keys.Control) != Keys.None) pos.line = int.MaxValue;
                        pos.column = int.MaxValue;
                        MoveTo(pos);
                        break;
                    }
                case Keys.PageUp:
                    {
                        Position pos = m_caretPosition;
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            pos.line = m_origin.line;
                        }
                        else
                        {
                            Position origin = m_origin;
                            origin.line -= NumVisibleLines;
                            ScrollTo(origin);
                            pos.line -= NumVisibleLines;
                        }
                        MoveTo(pos);
                        break;
                    }
                case Keys.PageDown:
                    {
                        Position pos = m_caretPosition;
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            pos.line = m_origin.line + NumVisibleLines - 1;
                        }
                        else
                        {
                            Position origin = m_origin;
                            origin.line += NumVisibleLines;
                            ScrollTo(origin);
                            pos.line += NumVisibleLines;
                        }
                        MoveTo(pos);
                        break;
                    }
                case Keys.Tab:
                    BeginUndo();
                    if (m_selection.IsEmpty)
                    {
                        if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                        {
                            if (m_caretPosition.column > 0 && m_lines[m_caretPosition.line].line[m_caretPosition.column - 1] == '\t')
                            {
                                Selection sel = new Selection(m_caretPosition, m_caretPosition);
                                sel.start.column--;
                                DeleteSelection(sel);
                            }
                        }
                        else
                        {
                            Paste("\t");
                        }
                    }
                    else
                    {
                        IndentSelection((Control.ModifierKeys & Keys.Shift) == 0, true);
                    }
                    EndUndo();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Back:
                    if (!m_selection.IsEmpty)
                    {
                        BeginUndo();
                        DeleteSelection();
                        EndUndo();
                    }
                    else
                    {
                        Selection sel = new Selection(m_caretPosition, m_caretPosition);
                        if (m_caretPosition.column > 0)
                        {
                            sel.start.column--;
                        }
                        else if (m_caretPosition.line > 0)
                        {
                            sel.start.line--;
                            sel.start.column = m_lines[sel.start.line].line.Length;
                        }
                        else
                        {
                            break;
                        }
                        BeginUndo();
                        DeleteSelection(sel);
                        EndUndo();
                    }
                    break;
                case Keys.Delete:
                    if (!m_selection.IsEmpty)
                    {
                        if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                        {
                            Cut();
                        }
                        else
                        {
                            BeginUndo();
                            DeleteSelection();
                            EndUndo();
                        }
                    }
                    else
                    {
                        Selection sel = new Selection(m_caretPosition, m_caretPosition);
                        if (m_caretPosition.column < m_lines[m_caretPosition.line].line.Length)
                        {
                            sel.end.column++;
                        }
                        else if (m_caretPosition.line < m_lines.Count - 1)
                        {
                            sel.end.line++;
                            sel.end.column = 0;
                        }
                        else
                        {
                            break;
                        }
                        BeginUndo();
                        DeleteSelection(sel);
                        EndUndo();
                    }
                    break;
                case Keys.Return:
                    {
                        string line = m_lines[m_caretPosition.line].line;
                        int i = 0;
                        while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
                        {
                            i++;
                        }
                        BeginUndo();
                        Paste("\n" + line.Substring(0, i));
                        EndUndo();
                        break;
                    }
                case Keys.A:
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None) SelectAll();
                    break;
                case Keys.C:
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None) Copy();
                    break;
                case Keys.V:
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None) Paste();
                    break;
                case Keys.X:
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None) Cut();
                    break;
                case Keys.Insert:
                    if ((Control.ModifierKeys & Keys.Shift) != Keys.None) Paste();
                    else if ((Control.ModifierKeys & Keys.Control) != Keys.None) Copy();
                    break;
                case Keys.Z:
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None) Undo();
                    break;
            }
            base.OnKeyDown(e);
        }

        protected void OnContentChanged(Position start, Position end, bool expand)
        {
            Position oldRef = expand ? start : end;
            Position newRef = expand ? end : start;
            OnContentChanged(oldRef, newRef);
        }

        protected void UpdatePosition(ref Position pos, Position oldRef, Position newRef)
        {
            if (pos >= oldRef)
            {
                pos = MoveRelativePosition(oldRef, newRef, pos);
            }
            ClipPosition(ref pos);
        }

        protected virtual void OnContentChanged(Position oldRef, Position newRef)
        {
            UpdatePosition(ref m_selection.start, oldRef, newRef);
            UpdatePosition(ref m_selection.end, oldRef, newRef);
            UpdatePosition(ref m_caretPosition, oldRef, newRef);
            SetCaretPosition(m_caretPosition, false);
            UpdatePosition(ref m_anchorPosition, oldRef, newRef);
            UpdateScrollbars();
            Invalidate();
        }

        protected void SetCaretPosition(Position position, bool anchor)
        {
            SetCaretPosition(position, anchor, true);
        }

        protected void SetCaretPosition(Position position, bool anchor, bool autoScroll)
        {
            ClipPosition(ref position);
            GetPointFromPosition(position, out int x, out int y);
            SetCaretPosition(position, x, y, anchor, autoScroll);
        }

        protected void SetCaretPosition(Position position, int x, int y, bool anchor, bool autoScroll)
        {
            if (m_selection.IsEmpty && m_selection.start == m_caretPosition)
            {
                m_selection = new Selection(position);
            }
            m_caretPosition = position;
            if (anchor)
            {
                m_anchorPosition = m_caretPosition;
            }

            if (x < LeftMarginWidth)
            {
                Win32.SetCaretPos(-1, -1);
            }
            else
            {
                Win32.SetCaretPos(x, y);
            }

            if (autoScroll)
            {
                int dxLeft = x - (base.ClientRectangle.Left + LeftMarginWidth);
                int dxRight = x - base.ClientRectangle.Right;
                int dyTop = y - base.ClientRectangle.Top;
                int dyBottom = y + LineHeight - base.ClientRectangle.Bottom;

                Position origin = m_origin;
                if (dxLeft < 0) origin.column += dxLeft / CharWidth - 1;
                else if (dxRight >= 0) origin.column += dxRight / CharWidth + 1;

                if (dyTop < 0) origin.line += dyTop / LineHeight - 1;
                else if (dyBottom >= 0) origin.line += dyBottom / LineHeight + 1;

                if (origin != m_origin)
                {
                    ScrollTo(origin);
                }
            }
            OnCaretPositionChanged();
        }

        protected virtual void OnCaretPositionChanged()
        {
            this.CaretPositionChanged?.Invoke(this, null);
        }

        protected void ClipPosition(ref Position position)
        {
            ClipPositionLine(ref position);
            ClipPositionColumn(ref position);
        }

        protected void ClipPositionLine(ref Position position)
        {
            if (position.line < 0) position.line = 0;
            else if (position.line >= m_lines.Count) position.line = m_lines.Count - 1;
        }

        protected void ClipPositionColumn(ref Position position)
        {
            if (position.column < 0) position.column = 0;
            else if (position.column > m_lines[position.line].line.Length) position.column = m_lines[position.line].line.Length;
        }

        protected Position MoveRelativePosition(Position oldRef, Position newRef, Position pos)
        {
            int colDelta = pos.column - oldRef.column;
            int lineDelta = pos.line - oldRef.line;
            Position result = new Position
            {
                line = newRef.line + lineDelta
            };
            if (lineDelta == 0)
            {
                result.column = newRef.column + colDelta;
            }
            else
            {
                result.column = pos.column;
            }
            return result;
        }

        protected void ClearSelection()
        {
            if (!m_selection.IsEmpty)
            {
                Invalidate();
            }
            m_selection.start = m_caretPosition;
            m_selection.end = m_caretPosition;
        }

        protected void SetSelection(Selection selection)
        {
            ClipPosition(ref selection.start);
            ClipPosition(ref selection.end);
            selection.Normalize();
            if (m_selection != selection)
            {
                m_selection = selection;
                Invalidate();
            }
        }

        protected void ExpandSelectionWords(ref Selection selection)
        {
            selection.Normalize();
            selection.start.column = FindWordStart(m_lines[selection.start.line].line, selection.start.column);
            selection.end.column = FindWordEnd(m_lines[selection.end.line].line, selection.end.column);
            Invalidate();
        }

        protected string DeleteSelection() => DeleteSelection(true);
        protected string DeleteSelection(bool undo)
        {
            string result = DeleteSelection(m_selection, undo);
            ClearSelection();
            return result;
        }
        protected string DeleteSelection(Selection selection) => DeleteSelection(selection, true);

        protected string DeleteSelection(Selection selection, bool undo)
        {
            if (selection.IsEmpty) return "";
            string selectionText = GetSelectionText(selection);
            if (undo)
            {
                AddUndoCommand(new UndoTextCommand(selection, selectionText, false));
            }
            m_lines[selection.start.line].line = m_lines[selection.start.line].line.Substring(0, selection.start.column) + m_lines[selection.end.line].line.Substring(selection.end.column);
            int startLine = selection.start.line + 1;
            int lineCount = selection.end.line - startLine + 1;
            if (startLine < m_lines.Count && lineCount > 0)
            {
                m_lines.ClearTags(startLine, lineCount);
                m_lines.RemoveRange(startLine, lineCount);
            }
            OnContentChanged(selection.end, selection.start, false);
            return selectionText;
        }

        protected void SelectAll()
        {
            m_selection.start = new Position(0, 0);
            m_selection.end = new Position(m_lines.Count - 1, m_lines[m_lines.Count - 1].line.Length);
            SetCaretPosition(m_selection.end, true);
            SetSelection(m_selection);
        }

        protected string GetSelectionText() => GetSelectionText(m_selection);

        protected string GetSelectionText(Selection selection)
        {
            if (selection.IsEmpty) return "";
            if (selection.start.line == selection.end.line)
            {
                return m_lines[selection.start.line].line.Substring(selection.start.column, selection.end.column - selection.start.column);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(m_lines[selection.start.line].line.Substring(selection.start.column));
            for (int i = selection.start.line + 1; i < selection.end.line; i++)
            {
                sb.AppendLine(m_lines[i].line);
            }
            sb.Append(m_lines[selection.end.line].line.Substring(0, selection.end.column));
            return sb.ToString();
        }

        protected void IndentSelection(bool indent, bool undo)
        {
            if (!indent)
            {
                for (int i = m_selection.start.line; i <= m_selection.end.line; i++)
                {
                    if (m_lines[i].line.Length > 0 && m_lines[i].line[0] == '\t')
                    {
                        DeleteSelection(new Selection(new Position(i, 0), new Position(i, 1)), false);
                    }
                }
            }
            else
            {
                for (int j = m_selection.start.line; j <= m_selection.end.line; j++)
                {
                    Paste("\t", new Position(j, 0), false);
                }
            }
            if (undo)
            {
                AddUndoCommand(new UndoIndentCommand(m_selection, indent));
            }
        }

        public void Cut()
        {
            Copy();
            BeginUndo();
            DeleteSelection();
            EndUndo();
        }

        public void Copy()
        {
            string text = GetSelectionText();
            if (text.Length == 0)
                Clipboard.Clear();
            else
                Clipboard.SetText(text);
        }

        public void Paste() => Paste(Clipboard.GetText());
        public void Paste(string text) => Paste(text, true);

        protected void Paste(string text, bool undo)
        {
            if (undo) BeginUndo();
            DeleteSelection(undo);
            Paste(text, m_caretPosition, undo);
            if (undo) EndUndo();
        }

        protected void Paste(string text, Position position) => Paste(text, position, true);

        protected void Paste(string text, Position position, bool undo)
        {
            if (text == null) return;
            string[] lines = text.Split(s_lineSeparators, StringSplitOptions.None);
            if (lines.Length == 0) return;

            ClipPosition(ref position);
            string endOfLine = m_lines[position.line].line.Substring(position.column);
            m_lines[position.line].line = m_lines[position.line].line.Substring(0, position.column) + lines[0];

            if (lines.Length > 1)
            {
                Line[] newLines = new Line[lines.Length - 1];
                for (int i = 0; i < newLines.Length; i++)
                {
                    newLines[i] = new Line(lines[i + 1]);
                }
                m_lines.InsertRange(position.line + 1, newLines);
            }

            int endLine = position.line + lines.Length - 1;
            Position endPos = new Position(endLine, m_lines[endLine].line.Length);
            m_lines[endLine].line += endOfLine;

            if (undo)
            {
                AddUndoCommand(new UndoTextCommand(new Selection(position, endPos), text, true));
            }
            OnContentChanged(position, endPos, true);
        }

        protected void ScrollTo(Position pos)
        {
            ClipPositionLine(ref pos);
            if (pos.column < 0) pos.column = 0;

            int dx = pos.column - m_origin.column;
            int dy = pos.line - m_origin.line;
            m_origin = pos;

            if (dx != 0)
            {
                Win32.Rect clipRect = new Win32.Rect(LeftMarginWidth, 0, base.ClientRectangle.Width - LeftMarginWidth, base.ClientRectangle.Height);
                Win32.ScrollWindowEx(base.Handle, -dx * CharWidth, 0, ref clipRect, ref clipRect, IntPtr.Zero, IntPtr.Zero, 2);
            }
            if (dy != 0)
            {
                Win32.Rect clipRect = new Win32.Rect(0, 0, base.ClientRectangle.Width, base.ClientRectangle.Height);
                Win32.ScrollWindowEx(base.Handle, 0, -dy * LineHeight, ref clipRect, ref clipRect, IntPtr.Zero, IntPtr.Zero, 2);
            }

            UpdateScrollbars();
            SetCaretPosition(m_caretPosition, false, false);
        }

        protected void UpdateScrollbars()
        {
            UpdateScrollbar(true);
            UpdateScrollbar(false);
        }

        protected void UpdateScrollbar(bool vertical)
        {
            int nBar = vertical ? 1 : 0; // SB_VERT : SB_HORZ
            Win32.ScrollInfo scrollInfo = new Win32.ScrollInfo { fMask = 31 }; // SIF_ALL
            Win32.GetScrollInfo(base.Handle, nBar, scrollInfo);

            if (vertical)
            {
                scrollInfo.nPage = NumVisibleLines; // <<< FIX HERE
                scrollInfo.nMin = 0;
                scrollInfo.nMax = m_lines.Count - 1;
                scrollInfo.nPos = m_origin.line;
            }
            else
            {
                scrollInfo.nPage = NumVisibleChars; // <<< FIX HERE
                scrollInfo.nMin = 0;
                scrollInfo.nMax = 200; // Arbitrary max width
                scrollInfo.nPos = m_origin.column;
            }

            if (scrollInfo.nMax < 0) scrollInfo.nMax = 0;
            if (scrollInfo.nPos < scrollInfo.nMin) scrollInfo.nPos = scrollInfo.nMin;
            if (scrollInfo.nPos > scrollInfo.nMax - scrollInfo.nPage + 1) scrollInfo.nPos = (int)(scrollInfo.nMax - scrollInfo.nPage + 1);
            if (scrollInfo.nPos < 0) scrollInfo.nPos = 0;

            Win32.SetScrollInfo(base.Handle, nBar, scrollInfo, true);
        }

        public void Undo()
        {
            if (m_undoEntries.Count > 0)
            {
                UndoEntry entry = m_undoEntries[m_undoEntries.Count - 1];
                entry.ToggleState(this);
                m_undoEntries.RemoveAt(m_undoEntries.Count - 1);
            }
        }

        protected void TrimUndo()
        {
            while (m_undoEntries.Count > 100)
            {
                m_undoEntries.RemoveAt(0);
            }
        }

        protected void BeginUndo()
        {
            if (undoLevel == 0)
            {
                if (m_undoEntry != null)
                {
                    EndUndo();
                }
                m_undoEntry = new UndoEntry();
                m_undoEntry.Start(this);
            }
            undoLevel++;
        }

        protected void AddUndoCommand(UndoCommand cmd)
        {
            m_undoEntry.AddCommand(cmd);
        }

        protected void EndUndo()
        {
            if (undoLevel == 1)
            {
                if (m_undoEntry.Stop(this))
                {
                    m_undoEntries.Add(m_undoEntry);
                }
                m_undoEntry = null;
                TrimUndo();
            }
            undoLevel--;
        }
    }
}