using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;

namespace FC2Editor.UI
{
    internal class NomadCodeBox : NomadTextBox
    {
        public interface IIcon : IDisposable
        {
            void Draw(Graphics g, Rectangle rect);
        }

        private CodeHelper m_codeHelper;
        private Selection m_codeHelperRange;

        private static Dictionary<string, string> s_keywords;
        private static string[] s_keywordList;
        private static Dictionary<string, Wilderness.FunctionDef> s_functions;

        static NomadCodeBox()
        {
            s_keywords = new Dictionary<string, string>();
            s_keywordList = new string[] { "if", "then", "else", "end", "for", "while", "do", "function", "local", "return", "break", "nil", "true", "false", "and", "or", "not" };
            s_functions = new Dictionary<string, Wilderness.FunctionDef>();
            foreach (string keyword in s_keywordList)
            {
                s_keywords.Add(keyword, keyword);
            }
        }

        protected override void OnPaintMargin(PaintEventArgs e)
        {
            base.OnPaintMargin(e);
            int line = m_origin.line;
            int num = m_origin.line + base.NumVisibleLines;
            if (num > m_lines.Count)
            {
                num = m_lines.Count;
            }
            int iconSize = Math.Min(base.LineHeight, base.LeftMarginWidth);
            int currentY = 0;
            for (int i = line; i < num; i++)
            {
                if (m_lines[i].tag is IIcon icon)
                {
                    icon.Draw(e.Graphics, new Rectangle(0, currentY, iconSize, iconSize));
                }
                currentY += base.LineHeight;
            }
        }

        public IIcon GetIcon(int line)
        {
            if (line >= 0 && line < m_lines.Count)
                return (IIcon)m_lines[line].tag;
            return null;
        }

        public void SetIcon(int line, IIcon icon)
        {
            if (line >= 0 && line < m_lines.Count)
            {
                m_lines[line].tag = icon;
                Invalidate();
            }
        }

        public void ClearIcons()
        {
            m_lines.ClearTags();
            Invalidate();
        }

        protected string GetToken(string s, int index)
        {
            index = SkipSpaceEnd(s, index);
            int wordEnd = FindWordEnd(s, index);
            if (wordEnd == index)
            {
                if (index < s.Length - 1 && s[index] == '-' && s[index + 1] == '-')
                    return "--";
                return null;
            }
            return s.Substring(index, wordEnd - index);
        }

        protected override void DrawTextFormat(Graphics g, IntPtr hFont, Position position, int x, int y, string s)
        {
            Color currentColor = ForeColor;
            int currentIndex = 0;
            while (currentIndex < s.Length)
            {
                int nonSpaceIndex = SkipSpaceEnd(s, currentIndex);
                string token = GetToken(s, nonSpaceIndex);

                if (token == null)
                {
                    break;
                }
                int tokenEndIndex = nonSpaceIndex + token.Length;

                if (token == "--")
                {
                    x += DrawText(g, hFont, new Position(position.line, position.column), x, y, s.Substring(position.column, nonSpaceIndex - position.column), currentColor);
                    currentColor = Color.Green;
                    position.column = nonSpaceIndex;
                    currentIndex = s.Length; // Comment goes to end of line
                    break;
                }

                Color tokenColor = s_keywords.ContainsKey(token) ? Color.Blue : (s_functions.ContainsKey(token) ? Color.Maroon : ForeColor);

                if (currentColor != tokenColor)
                {
                    x += DrawText(g, hFont, new Position(position.line, position.column), x, y, s.Substring(position.column, nonSpaceIndex - position.column), currentColor);
                    position.column = nonSpaceIndex;
                }

                x += DrawText(g, hFont, new Position(position.line, position.column), x, y, s.Substring(position.column, tokenEndIndex - position.column), tokenColor);

                currentColor = ForeColor;
                position.column = tokenEndIndex;
                currentIndex = tokenEndIndex;
            }
            DrawText(g, hFont, position, x, y, s.Substring(position.column), currentColor);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            switch (e.KeyChar)
            {
                case '(':
                    ShowCodeHelper();
                    break;
                case ')':
                    HideCodeHelper();
                    break;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None && (Control.ModifierKeys & Keys.Shift) != Keys.None)
                    {
                        ShowCodeHelper();
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Escape:
                    HideCodeHelper();
                    break;
                case Keys.Return:
                    HideCodeHelper();
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnContentChanged(Position oldRef, Position newRef)
        {
            UpdatePosition(ref m_codeHelperRange.start, oldRef, newRef);
            UpdatePosition(ref m_codeHelperRange.end, oldRef, newRef);
            base.OnContentChanged(oldRef, newRef);
        }

        protected override void OnCaretPositionChanged()
        {
            if (m_caretPosition <= m_codeHelperRange.start || m_caretPosition > m_codeHelperRange.end)
            {
                HideCodeHelper();
            }
            base.OnCaretPositionChanged();
        }

        public void ShowCodeHelper()
        {
            if (m_caretPosition.column == 0) return;

            string line = m_lines[m_caretPosition.line].line;
            int parenthesisIndex = line.LastIndexOf('(', m_caretPosition.column - 1);
            if (parenthesisIndex == -1) return;

            int wordStartIndex = FindWordStart(line, parenthesisIndex);
            string functionName = line.Substring(wordStartIndex, parenthesisIndex - wordStartIndex);

            if (s_functions.TryGetValue(functionName, out Wilderness.FunctionDef funcDef))
            {
                GetPointFromPosition(new Position(m_caretPosition.line, parenthesisIndex + 1), out int x, out int y);
                int closingParenthesisIndex = line.IndexOf(')', m_caretPosition.column - 1);
                if (closingParenthesisIndex == -1)
                {
                    closingParenthesisIndex = line.Length;
                }

                m_codeHelperRange = new Selection(new Position(m_caretPosition.line, parenthesisIndex), new Position(m_caretPosition.line, closingParenthesisIndex));

                if (m_codeHelper == null)
                {
                    m_codeHelper = new CodeHelper();
                }
                else
                {
                    m_codeHelper.Visible = false;
                }

                m_codeHelper.Setup(funcDef);
                m_codeHelper.Location = new Point(x, y + base.LineHeight);
                SuspendLayout();
                m_codeHelper.Parent = this;
                ResumeLayout();
                m_codeHelper.Visible = true;
            }
        }

        public void HideCodeHelper()
        {
            if (m_codeHelper != null)
            {
                m_codeHelper.Dispose();
                m_codeHelper = null;
            }
        }

        public static void InitFunctions()
        {
            s_functions.Clear();
            for (int i = 0; i < Wilderness.NumFunctions; i++)
            {
                Wilderness.FunctionDef function = Wilderness.GetFunction(i);
                s_functions.Add(function.Name, function);
            }
        }
    }
}