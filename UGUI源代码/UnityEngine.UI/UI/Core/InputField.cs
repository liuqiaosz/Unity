using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    /// <summary>
    /// Editable text input field.
    /// </summary>

    [AddComponentMenu("UI/Input Field", 31)]
    public class InputField
        : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ISubmitHandler,
        ICanvasElement
    {
        public enum ContentType
        {
            Standard,
            Autocorrected,
            IntegerNumber,
            DecimalNumber,
            Alphanumeric,
            Name,
            EmailAddress,
            Password,
            Pin,
            Custom
        }

        public enum InputType
        {
            Standard,
            AutoCorrect,
            Password,
        }

        public enum CharacterValidation
        {
            None,
            Integer,
            Decimal,
            Alphanumeric,
            Name,
            EmailAddress
        }

        public enum LineType
        {
            SingleLine,
            MultiLineSubmit,
            MultiLineNewline
        }

        public delegate char OnValidateInput(string text, int charIndex, char addedChar);

        [Serializable]
        public class SubmitEvent : UnityEvent<string> { }

        [Serializable]
        public class OnChangeEvent : UnityEvent<string> { }

        static protected TouchScreenKeyboard m_Keyboard;
        static private readonly char[] kSeparators = { ' ', '.', ',' };

        #region Exposed properties
        /// <summary>
        /// Text Text used to display the input's value.
        /// </summary>

        [SerializeField]
        [FormerlySerializedAs("text")]
        protected Text m_TextComponent;

        [SerializeField]
        protected Graphic m_Placeholder;

        [SerializeField]
        private ContentType m_ContentType = ContentType.Standard;

        /// <summary>
        /// Type of data expected by the input field.
        /// </summary>
        [FormerlySerializedAs("inputType")]
        [SerializeField]
        private InputType m_InputType = InputType.Standard;

        /// <summary>
        /// The character used to hide text in password field.
        /// </summary>
        [FormerlySerializedAs("asteriskChar")]
        [SerializeField]
        private char m_AsteriskChar = '*';

        /// <summary>
        /// Keyboard type applies to mobile keyboards that get shown.
        /// </summary>
        [FormerlySerializedAs("keyboardType")]
        [SerializeField]
        private TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

        [SerializeField]
        private LineType m_LineType = LineType.SingleLine;

        /// <summary>
        /// Should hide mobile input.
        /// </summary>

        [FormerlySerializedAs("hideMobileInput")]
        [SerializeField]
        private bool m_HideMobileInput = false;

        /// <summary>
        /// What kind of validation to use with the input field's data.
        /// </summary>
        [FormerlySerializedAs("validation")]
        [SerializeField]
        private CharacterValidation m_CharacterValidation = CharacterValidation.None;

        /// <summary>
        /// Maximum number of characters allowed before input no longer works.
        /// </summary>
        [FormerlySerializedAs("characterLimit")]
        [SerializeField]
        private int m_CharacterLimit = 0;

        /// <summary>
        /// Event delegates triggered when the input field submits its data.
        /// </summary>
        [FormerlySerializedAs("onSubmit")]
        [FormerlySerializedAs("m_OnSubmit")]
        [SerializeField]
        private SubmitEvent m_EndEdit = new SubmitEvent();

        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [FormerlySerializedAs("onValueChange")]
        [SerializeField]
        private OnChangeEvent m_OnValueChange = new OnChangeEvent();

        /// <summary>
        /// Custom validation callback.
        /// </summary>
        [FormerlySerializedAs("onValidateInput")]
        [SerializeField]
        private OnValidateInput m_OnValidateInput;

        [FormerlySerializedAs("selectionColor")]
        [SerializeField]
        private Color m_SelectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        /// <summary>
        /// Input field's value.
        /// </summary>

        [SerializeField]
        [FormerlySerializedAs("mValue")]
        protected string m_Text = string.Empty;

        [SerializeField]
        [Range(0f, 8f)]
        private float m_CaretBlinkRate = 1.7f;

        #endregion

        protected int m_CaretPosition = 0;
        protected int m_CaretSelectPosition = 0;
        private RectTransform caretRectTrans = null;
        protected UIVertex[] m_CursorVerts = null;
        private TextGenerator m_InputTextCache;
        private CanvasRenderer m_CachedInputRenderer;
        private readonly List<UIVertex> m_Vbo = new List<UIVertex>();
        private bool m_AllowInput = false;
        private bool m_ShouldActivateNextUpdate = false;
        private bool m_UpdateDrag = false;
        private bool m_DragPositionOutOfBounds = false;
        private const float kHScrollSpeed = 0.05f;
        private const float kVScrollSpeed = 0.10f;
        protected bool m_CaretVisible;
        private Coroutine m_BlickCoroutine = null;
        private float m_BlinkStartTime = 0.0f;
        protected int m_DrawStart = 0;
        protected int m_DrawEnd = 0;
        private Coroutine m_DragCoroutine = null;
        private string m_OriginalText = "";
        private bool m_WasCanceled = false;
        private bool m_HasDoneFocusTransition = false;

        // Doesn't include dot and @ on purpose! See usage for details.
        const string kEmailSpecialCharacters = "!#$%&'*+-/=?^_`{|}~";

        protected InputField()
        { }


        protected TextGenerator cachedInputTextGenerator
        {
            get
            {
                if (m_InputTextCache == null)
                    m_InputTextCache = new TextGenerator();

                return m_InputTextCache;
            }
        }

        /// <summary>
        /// Should the mobile keyboard input be hidden.
        /// </summary>

        public bool shouldHideMobileInput
        {
            set
            {
                SetPropertyUtility.SetStruct(ref m_HideMobileInput, value);
            }
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.BlackBerryPlayer:
                    case RuntimePlatform.IPhonePlayer:
                        return m_HideMobileInput;
                }

                return true;
            }
        }

        /// <summary>
        /// Input field's current text value.
        /// </summary>

        public string text
        {
            get
            {
                if (m_Keyboard != null && m_Keyboard.active && !InPlaceEditing())
                    return m_Keyboard.text;

                return m_Text;
            }
            set
            {
                if (this.text == value)
                    return;

                m_Text = value;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    SendOnValueChangedAndUpdateLabel();
                    return;
                }
#endif

                if (m_Keyboard != null)
                    m_Keyboard.text = m_Text;

                if (m_CaretPosition > m_Text.Length)
                    m_CaretPosition = m_CaretSelectPosition = m_Text.Length;

                SendOnValueChangedAndUpdateLabel();
            }
        }

        public bool isFocused
        {
            get { return m_AllowInput; }
        }

        public float caretBlinkRate
        {
            get { return m_CaretBlinkRate; }
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_CaretBlinkRate, value))
                {
                    if (m_AllowInput)
                        SetCaretActive();
                }
            }
        }

        public Text textComponent { get { return m_TextComponent; } set { SetPropertyUtility.SetClass(ref m_TextComponent, value); } }

        public Graphic placeholder { get { return m_Placeholder; } set { SetPropertyUtility.SetClass(ref m_Placeholder, value); } }

        public Color selectionColor { get { return m_SelectionColor; } set { SetPropertyUtility.SetColor(ref m_SelectionColor, value); } }

        public SubmitEvent onEndEdit { get { return m_EndEdit; } set { SetPropertyUtility.SetClass(ref m_EndEdit, value); } }

        public OnChangeEvent onValueChange { get { return m_OnValueChange; } set { SetPropertyUtility.SetClass(ref m_OnValueChange, value); } }

        public OnValidateInput onValidateInput { get { return m_OnValidateInput; } set { SetPropertyUtility.SetClass(ref m_OnValidateInput, value); } }

        public int characterLimit { get { return m_CharacterLimit; } set { SetPropertyUtility.SetStruct(ref m_CharacterLimit, value); } }

        // Content Type related

        public ContentType contentType { get { return m_ContentType; } set { if (SetPropertyUtility.SetStruct(ref m_ContentType, value)) EnforceContentType(); } }

        public LineType lineType { get { return m_LineType; } set { if (SetPropertyUtility.SetStruct(ref m_LineType, value)) SetToCustomIfContentTypeIsNot(ContentType.Standard, ContentType.Autocorrected); } }

        public InputType inputType { get { return m_InputType; } set { if (SetPropertyUtility.SetStruct(ref m_InputType, value)) SetToCustom(); } }

        public TouchScreenKeyboardType keyboardType { get { return m_KeyboardType; } set { if (SetPropertyUtility.SetStruct(ref m_KeyboardType, value)) SetToCustom(); } }

        public CharacterValidation characterValidation { get { return m_CharacterValidation; } set { if (SetPropertyUtility.SetStruct(ref m_CharacterValidation, value)) SetToCustom(); } }

        // Derived property
        public bool multiLine { get { return m_LineType == LineType.MultiLineNewline || lineType == LineType.MultiLineSubmit; } }
        // Not shown in Inspector.
        public char asteriskChar { get { return m_AsteriskChar; } set { SetPropertyUtility.SetStruct(ref m_AsteriskChar, value); } }
        public bool wasCanceled { get { return m_WasCanceled; } }

        protected void ClampPos(ref int pos)
        {
            if (pos < 0) pos = 0;
            else if (pos > text.Length) pos = text.Length;
        }

        /// <summary>
        /// Current position of the cursor.
        /// </summary>

        protected int caretPosition { get { return m_CaretPosition + Input.compositionString.Length; } set { m_CaretPosition = value; ClampPos(ref m_CaretPosition); } }
        protected int caretSelectPos { get { return m_CaretSelectPosition + Input.compositionString.Length; } set { m_CaretSelectPosition = value; ClampPos(ref m_CaretSelectPosition); } }
        private bool hasSelection { get { return caretPosition != caretSelectPos; } }

    #if UNITY_EDITOR
        // Remember: This is NOT related to text validation!
        // This is Unity's own OnValidate method which is invoked when changing values in the Inspector.
        protected override void OnValidate()
        {
            base.OnValidate();
            EnforceContentType();
            UpdateLabel();
            if (m_AllowInput)
                SetCaretActive();
        }

    #endif // if UNITY_EDITOR

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_Text == null)
                m_Text = string.Empty;
            m_DrawStart = 0;
            m_DrawEnd = m_Text.Length;
            if (m_TextComponent != null)
            {
                m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
                UpdateLabel();
            }
        }

        protected override void OnDisable()
        {
            DeactivateInputField();
            if (m_TextComponent != null)
            {
                m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
                m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
            }
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_CachedInputRenderer)
                m_CachedInputRenderer.SetVertices(null, 0);

            base.OnDisable();
        }

        IEnumerator CaretBlink()
        {
            // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
            m_CaretVisible = true;
            yield return null;

            while (isFocused && m_CaretBlinkRate > 0)
            {
                // Update caret state as first thing after waiting.
                // This also ensures m_CaretBlinkRate didn't get set to 0 in between
                // checking it above and running this time comparison.
                if (m_BlinkStartTime + (1f / m_CaretBlinkRate) < Time.unscaledTime)
                {
                    m_CaretVisible = !m_CaretVisible;
                    m_BlinkStartTime = Time.unscaledTime;

                    UpdateGeometry();
                }

                // Then wait again.
                yield return null;
            }
            m_BlickCoroutine = null;
        }

        void SetCaretVisible()
        {
            if (!m_AllowInput)
                return;

            m_CaretVisible = true;
            m_BlinkStartTime = Time.unscaledTime;
            SetCaretActive();
        }

        // SetCaretActive will not set the caret immediately visible - it will wait for the next time to blink.
        // However, it will handle things correctly if the blink speed changed from zero to non-zero or non-zero to zero.
        void SetCaretActive()
        {
            if (!m_AllowInput)
                return;

            if (m_CaretBlinkRate > 0.0f)
            {
                if (m_BlickCoroutine == null)
                    m_BlickCoroutine = StartCoroutine(CaretBlink());
            }
            else
            {
                m_CaretVisible = true;
            }
        }

        protected void OnFocus()
        {
            SelectAll();
        }

        protected void SelectAll()
        {
            caretPosition = text.Length;
            caretSelectPos = 0;
        }

        public void MoveTextEnd(bool shift)
        {
            int position = text.Length;

            if (shift)
            {
                caretSelectPos = position;
            }
            else
            {
                caretPosition = position;
                caretSelectPos = caretPosition;
            }
            UpdateLabel();
        }

        public void MoveTextStart(bool shift)
        {
            int position = 0;

            if (shift)
            {
                caretSelectPos = position;
            }
            else
            {
                caretPosition = position;
                caretSelectPos = caretPosition;
            }

            UpdateLabel();
        }

        // TODO: Why doesnt this use GUIUtility.systemCopyBuffer??
        static string clipboard
        {
            get
            {
                TextEditor te = new TextEditor();
                te.Paste();
                return te.content.text;
            }
            set
            {
                TextEditor te = new TextEditor();
                te.content = new GUIContent(value);
                te.OnFocus();
                te.Copy();
            }
        }

        private bool InPlaceEditing()
        {
            return !TouchScreenKeyboard.isSupported;
        }

        /// <summary>
        /// Update the text based on input.
        /// </summary>
        // TODO: Make LateUpdate a coroutine instead. Allows us to control the update to only be when the field is active.
        protected virtual void LateUpdate()
        {
            // Only activate if we are not already activated.
            if (m_ShouldActivateNextUpdate)
            {
                if (!isFocused)
                {
                    ActivateInputFieldInternal();
                    m_ShouldActivateNextUpdate = false;
                    return;
                }

                // Reset as we are already activated.
                m_ShouldActivateNextUpdate = false;
            }

            if (InPlaceEditing() || !isFocused)
                return;

            AssignPositioningIfNeeded();

            if (m_Keyboard == null || !m_Keyboard.active)
            {
                if (m_Keyboard != null && m_Keyboard.wasCanceled)
                    m_WasCanceled = true;

                OnDeselect(null);
                return;
            }

            string val = m_Keyboard.text;

            if (m_Text != val)
            {
                m_Text = "";

                for (int i = 0; i < val.Length; ++i)
                {
                    char c = val[i];

                    if (c == '\r' || (int)c == 3)
                        c = '\n';

                    if (onValidateInput != null)
                        c = onValidateInput(m_Text, m_Text.Length, c);
                    else if (characterValidation != CharacterValidation.None)
                        c = Validate(m_Text, m_Text.Length, c);

                    if (lineType == LineType.MultiLineSubmit && c == '\n')
                    {
                        m_Keyboard.text = m_Text;

                        OnDeselect(null);
                        return;
                    }

                    if (c != 0)
                        m_Text += c;
                }

                if (characterLimit > 0 && m_Text.Length > characterLimit)
                    m_Text = m_Text.Substring(0, characterLimit);
                caretPosition = caretSelectPos = m_Text.Length;

                // Set keyboard text before updating label, as we might have changed it with validation
                // and update label will take the old value from keyboard if we don't change it here
                if (m_Text != val)
                    m_Keyboard.text = m_Text;

                SendOnValueChangedAndUpdateLabel();
            }

            if (m_Keyboard.done)
            {
                if (m_Keyboard.wasCanceled)
                    m_WasCanceled = true;

                OnDeselect(null);
            }
        }

        public Vector2 ScreenToLocal(Vector2 screen)
        {
            var theCanvas = m_TextComponent.canvas;
            if (theCanvas == null)
                return screen;

            Vector3 pos = Vector3.zero;
            if (theCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                pos = m_TextComponent.transform.InverseTransformPoint(screen);
            }
            else if (theCanvas.worldCamera != null)
            {
                Ray mouseRay = theCanvas.worldCamera.ScreenPointToRay(screen);
                float dist;
                Plane plane = new Plane(m_TextComponent.transform.forward, m_TextComponent.transform.position);
                plane.Raycast(mouseRay, out dist);
                pos = m_TextComponent.transform.InverseTransformPoint(mouseRay.GetPoint(dist));
            }
            return new Vector2(pos.x, pos.y);
        }

        private int GetUnclampedCharacterLineFromPosition(Vector2 pos, TextGenerator generator)
        {
            if (!multiLine)
                return 0;

            float height = m_TextComponent.rectTransform.rect.yMax;

            // Position is above first line.
            if (pos.y > height)
                return -1;

            for (int i = 0; i < generator.lineCount; ++i)
            {
                float lineHeight = generator.lines[i].height / m_TextComponent.pixelsPerUnit;
                if (pos.y <= height && pos.y > (height - lineHeight))
                    return i;
                height -= lineHeight;
            }

            // Position is after last line.
            return generator.lineCount;
        }

        /// <summary>
        /// Given an input position in local space on the Text return the index for the selection cursor at this position.
        /// </summary>

        protected int GetCharacterIndexFromPosition(Vector2 pos)
        {
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            if (gen.lineCount == 0)
                return 0;

            int line = GetUnclampedCharacterLineFromPosition(pos, gen);
            if (line < 0)
                return 0;
            if (line >= gen.lineCount)
                return gen.characterCountVisible;

            int startCharIndex = gen.lines[line].startCharIdx;
            int endCharIndex = GetLineEndPosition(gen, line);

            for (int i = startCharIndex; i < endCharIndex; i++)
            {
                if (i >= gen.characterCountVisible)
                    break;

                UICharInfo charInfo = gen.characters[i];
                Vector2 charPos = charInfo.cursorPos / m_TextComponent.pixelsPerUnit;

                float distToCharStart = pos.x - charPos.x;
                float distToCharEnd = charPos.x + (charInfo.charWidth / m_TextComponent.pixelsPerUnit) - pos.x;
                if (distToCharStart < distToCharEnd)
                    return i;
            }

            return endCharIndex;
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() &&
                   IsInteractable() &&
                   eventData.button == PointerEventData.InputButton.Left &&
                   m_TextComponent != null &&
                   m_Keyboard == null;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);
            caretSelectPos = GetCharacterIndexFromPosition(localMousePos) + m_DrawStart;
            MarkGeometryAsDirty();

            m_DragPositionOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(textComponent.rectTransform, eventData.position, eventData.pressEventCamera);
            if (m_DragPositionOutOfBounds && m_DragCoroutine == null)
                m_DragCoroutine = StartCoroutine(MouseDragOutsideRect(eventData));

            eventData.Use();
        }

        IEnumerator MouseDragOutsideRect(PointerEventData eventData)
        {
            while (m_UpdateDrag && m_DragPositionOutOfBounds)
            {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out localMousePos);

                Rect rect = textComponent.rectTransform.rect;

                if (multiLine)
                {
                    if (localMousePos.y > rect.yMax)
                        MoveUp(true, true);
                    else if (localMousePos.y < rect.yMin)
                        MoveDown(true, true);
                }
                else
                {
                    if (localMousePos.x < rect.xMin)
                        MoveLeft(true, false);
                    else if (localMousePos.x > rect.xMax)
                        MoveRight(true, false);
                }
                UpdateLabel();
                float delay = multiLine ? kVScrollSpeed : kHScrollSpeed;
                yield return new WaitForSeconds(delay);
            }
            m_DragCoroutine = null;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            m_UpdateDrag = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            bool hadFocusBefore = m_AllowInput;
            base.OnPointerDown(eventData);

            if (!InPlaceEditing())
            {
                if (m_Keyboard == null || !m_Keyboard.active)
                {
                    OnSelect(eventData);
                    return;
                }
            }

            // Only set caret position if we didn't just get focus now.
            // Otherwise it will overwrite the select all on focus.
            if (hadFocusBefore)
            {
                Vector2 pos = ScreenToLocal(eventData.position);
                caretSelectPos = caretPosition = GetCharacterIndexFromPosition(pos) + m_DrawStart;
            }
            UpdateLabel();
            eventData.Use();
        }

        protected enum EditState
        {
            Continue,
            Finish
        }

        protected EditState KeyPressed(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            RuntimePlatform rp = Application.platform;
            bool isMac = (rp == RuntimePlatform.OSXEditor || rp == RuntimePlatform.OSXPlayer || rp == RuntimePlatform.OSXWebPlayer);
            bool ctrl = isMac ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                {
                    Backspace();
                    return EditState.Continue;
                }

                case KeyCode.Delete:
                {
                    ForwardSpace();
                    return EditState.Continue;
                }

                case KeyCode.Home:
                {
                    MoveTextStart(shift);
                    return EditState.Continue;
                }

                case KeyCode.End:
                {
                    MoveTextEnd(shift);
                    return EditState.Continue;
                }

                // Select All
                case KeyCode.A:
                {
                    if (ctrl)
                    {
                        SelectAll();
                        return EditState.Continue;
                    }
                    break;
                }

                // Copy
                case KeyCode.C:
                {
                    if (ctrl)
                    {
                        clipboard = GetSelectedString();
                        return EditState.Continue;
                    }
                    break;
                }

                // Paste
                case KeyCode.V:
                {
                    if (ctrl)
                    {
                        Append(clipboard);
                        return EditState.Continue;
                    }
                    break;
                }

                // Cut
                case KeyCode.X:
                {
                    if (ctrl)
                    {
                        clipboard = GetSelectedString();
                        Delete();
                        SendOnValueChangedAndUpdateLabel();
                        return EditState.Continue;
                    }
                    break;
                }

                case KeyCode.LeftArrow:
                {
                    MoveLeft(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.RightArrow:
                {
                    MoveRight(shift, ctrl);
                    return EditState.Continue;
                }

                case KeyCode.UpArrow:
                {
                    MoveUp(shift);
                    return EditState.Continue;
                }

                case KeyCode.DownArrow:
                {
                    MoveDown(shift);
                    return EditState.Continue;
                }

                // Submit
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                {
                    if (lineType != LineType.MultiLineNewline)
                    {
                        return EditState.Finish;
                    }
                    break;
                }

                case KeyCode.Escape:
                {
                    m_WasCanceled = true;
                    return EditState.Finish;
                }
            }

            // Dont allow return chars to be entered into single line fields.
            if (!multiLine && (evt.character == '\t'))
                return EditState.Continue;

            char c = evt.character;
            // Convert carriage return and end-of-text characters to newline.
            if (c == '\r' || (int)c == 3)
                c = '\n';

            if (IsValidChar(c))
            {
                Append(c);
            }

            if (c == 0)
            {
                if (Input.compositionString.Length > 0)
                {
                    UpdateLabel();
                }
            }
            return EditState.Continue;
        }

        private bool IsValidChar(char c)
        {
            // Delete key on mac
            if ((int)c == 127)
                return false;
            // Accept newline and tab
            if (c == '\t' || c == '\n')
                return true;

            return m_TextComponent.font.HasCharacter(c);
        }

        /// <summary>
        /// Handle the specified event.
        /// </summary>
        private Event m_ProcessingEvent = new Event();

        public void ProcessEvent(Event e)
        {
            KeyPressed(e);
        }

        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            bool consumedEvent = false;
            while (Event.PopEvent(m_ProcessingEvent))
            {
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    consumedEvent = true;
                    var shouldContinue = KeyPressed(m_ProcessingEvent);
                    if (shouldContinue == EditState.Finish)
                    {
                        DeactivateInputField();
                        break;
                    }
                }
            }

            if (consumedEvent)
                UpdateLabel();

            eventData.Use();
        }

        private string GetSelectedString()
        {
            if (!hasSelection)
                return "";

            int startPos = caretPosition;
            int endPos = caretSelectPos;

            // Ensure pos is always less then selPos to make the code simpler
            if (startPos > endPos)
            {
                int temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = startPos; i < endPos; ++i)
            {
                builder.Append(text[i]);
            }
            return builder.ToString();
        }

        private int FindtNextWordBegin()
        {
            if (caretSelectPos + 1 >= text.Length)
                return text.Length;

            int spaceLoc = text.IndexOfAny(kSeparators, caretSelectPos + 1);

            if (spaceLoc == -1)
                spaceLoc = text.Length;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveRight(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move right without holding shift,
                // we just place the cursor at the end.
                caretPosition = caretSelectPos = Mathf.Max(caretPosition, caretSelectPos);
                return;
            }

            int position;
            if (ctrl)
                position = FindtNextWordBegin();
            else
                position = caretSelectPos + 1;

            if (shift)
                caretSelectPos = position;
            else
                caretSelectPos = caretPosition = position;
        }

        private int FindtPrevWordBegin()
        {
            if (caretSelectPos - 2 < 0)
                return 0;

            int spaceLoc = text.LastIndexOfAny(kSeparators, caretSelectPos - 2);

            if (spaceLoc == -1)
                spaceLoc = 0;
            else
                spaceLoc++;

            return spaceLoc;
        }

        private void MoveLeft(bool shift, bool ctrl)
        {
            if (hasSelection && !shift)
            {
                // By convention, if we have a selection and move left without holding shift,
                // we just place the cursor at the start.
                caretPosition = caretSelectPos = Mathf.Min(caretPosition, caretSelectPos);
                return;
            }

            int position;
            if (ctrl)
                position = FindtPrevWordBegin();
            else
                position = caretSelectPos - 1;

            if (shift)
                caretSelectPos = position;
            else
                caretSelectPos = caretPosition = position;
        }

        private int DetermineCharacterLine(int charPos, TextGenerator generator)
        {
            if (!multiLine)
                return 0;

            for (int i = 0; i < generator.lineCount - 1; ++i)
            {
                if (generator.lines[i + 1].startCharIdx > charPos)
                    return i;
            }

            return generator.lineCount - 1;
        }

        /// <summary>
        ///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
        /// </summary>

        private int LineUpCharacterPosition(int originalPos, bool goToFirstChar)
        {
            if (originalPos >= cachedInputTextGenerator.characterCountVisible)
                return 0;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the last line return last character
            if (originLine - 1 < 0)
                return goToFirstChar ? 0 : originalPos;


            int endCharIdx = cachedInputTextGenerator.lines[originLine].startCharIdx - 1;

            for (int i = cachedInputTextGenerator.lines[originLine - 1].startCharIdx; i < endCharIdx; ++i)
            {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
                    return i;
            }
            return endCharIdx;
        }

        /// <summary>
        ///  Use cachedInputTextGenerator as the y component for the UICharInfo is not required
        /// </summary>

        private int LineDownCharacterPosition(int originalPos, bool goToLastChar)
        {
            if (originalPos >= cachedInputTextGenerator.characterCountVisible)
                return text.Length;

            UICharInfo originChar = cachedInputTextGenerator.characters[originalPos];
            int originLine = DetermineCharacterLine(originalPos, cachedInputTextGenerator);

            // We are on the last line return last character
            if (originLine + 1 >= cachedInputTextGenerator.lineCount)
                return goToLastChar ? text.Length : originalPos;

            // Need to determine end line for next line.
            int endCharIdx = GetLineEndPosition(cachedInputTextGenerator, originLine + 1);

            for (int i = cachedInputTextGenerator.lines[originLine + 1].startCharIdx; i < endCharIdx; ++i)
            {
                if (cachedInputTextGenerator.characters[i].cursorPos.x >= originChar.cursorPos.x)
                    return i;
            }
            return endCharIdx;
        }

        private void MoveDown(bool shift)
        {
            MoveDown(shift, true);
        }

        private void MoveDown(bool shift, bool goToLastChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press down without shift,
                // set caret position to end of selection before we move it down.
                caretPosition = caretSelectPos = Mathf.Max(caretPosition, caretSelectPos);
            }

            int position = multiLine ? LineDownCharacterPosition(caretSelectPos, goToLastChar) : text.Length;

            if (shift)
                caretSelectPos = position;
            else
                caretPosition = caretSelectPos = position;
        }

        private void MoveUp(bool shift)
        {
            MoveUp(shift, true);
        }

        private void MoveUp(bool shift, bool goToFirstChar)
        {
            if (hasSelection && !shift)
            {
                // If we have a selection and press up without shift,
                // set caret position to start of selection before we move it up.
                caretPosition = caretSelectPos = Mathf.Min(caretPosition, caretSelectPos);
            }

            int position = multiLine ? LineUpCharacterPosition(caretSelectPos, goToFirstChar) : 0;

            if (shift)
                caretSelectPos = position;
            else
                caretSelectPos = caretPosition = position;
        }

        private void Delete()
        {
            if (caretPosition == caretSelectPos)
                return;

            if (caretPosition < caretSelectPos)
            {
                m_Text = text.Substring(0, caretPosition) + text.Substring(caretSelectPos, text.Length - caretSelectPos);
                caretSelectPos = caretPosition;
            }
            else
            {
                m_Text = text.Substring(0, caretSelectPos) + text.Substring(caretPosition, text.Length - caretPosition);
                caretPosition = caretSelectPos;
            }
        }

        private void ForwardSpace()
        {
            if (hasSelection)
            {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (caretPosition < text.Length)
                {
                    m_Text = text.Remove(caretPosition, 1);
                    SendOnValueChangedAndUpdateLabel();
                }
            }
        }

        private void Backspace()
        {
            if (hasSelection)
            {
                Delete();
                SendOnValueChangedAndUpdateLabel();
            }
            else
            {
                if (caretPosition > 0)
                {
                    m_Text = text.Remove(caretPosition - 1, 1);
                    caretSelectPos = caretPosition = caretPosition - 1;
                    SendOnValueChangedAndUpdateLabel();
                }
            }
        }

        // Insert the character and update the label.
        private void Insert(char c)
        {
            string replaceString = c.ToString();
            Delete();

            // Can't go past the character limit
            if (characterLimit > 0 && text.Length >= characterLimit)
                return;

            m_Text = text.Insert(m_CaretPosition, replaceString);
            caretSelectPos = caretPosition += replaceString.Length;

            SendOnValueChanged();
        }

        private void SendOnValueChangedAndUpdateLabel()
        {
            SendOnValueChanged();
            UpdateLabel();
        }

        private void SendOnValueChanged()
        {
            if (onValueChange != null)
                onValueChange.Invoke(text);
        }

        /// <summary>
        /// Submit the input field's text.
        /// </summary>

        protected void SendOnSubmit()
        {
            if (onEndEdit != null)
                onEndEdit.Invoke(m_Text);
        }

        /// <summary>
        /// Append the specified text to the end of the current.
        /// </summary>

        protected virtual void Append(string input)
        {
            if (!InPlaceEditing())
                return;

            for (int i = 0, imax = input.Length; i < imax; ++i)
            {
                char c = input[i];

                if (c >= ' ')
                {
                    Append(c);
                }
            }
        }

        protected virtual void Append(char input)
        {
            if (!InPlaceEditing())
                return;

            // If we have an input validator, validate the input first
            if (onValidateInput != null)
                input = onValidateInput(text, caretPosition, input);
            else if (characterValidation != CharacterValidation.None)
                input = Validate(text, caretPosition, input);

            // If the input is invalid, skip it
            if (input == 0)
                return;

            // Append the character and update the label
            Insert(input);
        }

        /// <summary>
        /// Update the visual text Text.
        /// </summary>

        protected void UpdateLabel()
        {
            if (m_TextComponent != null && m_TextComponent.font != null)
            {
                string fullText;
                if (Input.compositionString.Length > 0)
                    fullText = text.Substring(0, m_CaretPosition) + Input.compositionString + text.Substring(m_CaretPosition);
                else
                    fullText = text;

                string processed;
                if (inputType == InputType.Password)
                    processed = new string(asteriskChar, fullText.Length);
                else
                    processed = fullText;

                bool isEmpty = string.IsNullOrEmpty(fullText);

                if (m_Placeholder != null)
                    m_Placeholder.enabled = isEmpty;

                // If not currently editing the text, set the visible range to the whole text.
                // The UpdateLabel method will then truncate it to the part that fits inside the Text area.
                // We can't do this when text is being edited since it would discard the current scroll,
                // which is defined by means of the m_DrawStart and m_DrawEnd indices.
                if (!m_AllowInput)
                {
                    m_DrawStart = 0;
                    m_DrawEnd = m_Text.Length;
                }

                if (!isEmpty)
                {
                    // Determine what will actually fit into the given line
                    Vector2 extents = m_TextComponent.rectTransform.rect.size;

                    var settings = m_TextComponent.GetGenerationSettings(extents);
                    settings.generateOutOfBounds = true;
                    cachedInputTextGenerator.Populate(processed, settings);

                    SetDrawRangeToContainCaretPosition(cachedInputTextGenerator, caretSelectPos, ref m_DrawStart, ref m_DrawEnd);

                    processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);

                    SetCaretVisible();
                }
                m_TextComponent.text = processed;

                MarkGeometryAsDirty();
            }
        }

        private bool IsSelectionVisible()
        {
            if (m_DrawStart > caretPosition || m_DrawStart > caretSelectPos)
                return false;

            if (m_DrawEnd < caretPosition || m_DrawEnd < caretSelectPos)
                return false;

            return true;
        }

        private static int GetLineStartPosition(TextGenerator gen, int line)
        {
            line = Mathf.Clamp(line, 0, gen.lines.Count - 1);
            return gen.lines[line].startCharIdx;
        }

        private static int GetLineEndPosition(TextGenerator gen, int line)
        {
            line = Mathf.Max(line, 0);
            if (line + 1 < gen.lines.Count)
                return gen.lines[line + 1].startCharIdx;
            return gen.characterCountVisible;
        }

        private void SetDrawRangeToContainCaretPosition(TextGenerator gen, int caretPos, ref int drawStart, ref int drawEnd)
        {
            // the extents gets modified by the pixel density, so we need to use the generated extents since that will be in the same 'space' as
            // the values returned by the TextGenerator.lines[x].height for instance.
            Vector2 extents = gen.rectExtents.size;
            if (multiLine)
            {
                var lines = gen.lines;
                int caretLine = DetermineCharacterLine(caretPos, gen);
                int height = (int)extents.y;

                // Have to compare with less or equal rather than just less.
                // The reason is that if the caret is between last char of one line and first char of next,
                // we want to interpret it as being on the next line.
                // This is also consistent with what DetermineCharacterLine returns.
                if (drawEnd <= caretPos)
                {
                    // Caret comes after drawEnd, so we need to move drawEnd to a later line end that comes after caret.
                    drawEnd = GetLineEndPosition(gen, caretLine);
                    for (int i = caretLine; i >= 0 && i < lines.Count; --i)
                    {
                        height -= lines[i].height;
                        if (height < 0)
                            break;

                        drawStart = GetLineStartPosition(gen, i);
                    }
                }
                else
                {
                    if (drawStart > caretPos)
                    {
                        // Caret comes before drawStart, so we need to move drawStart to an earlier line start that comes before caret.
                        drawStart = GetLineStartPosition(gen, caretLine);
                    }

                    int startLine = DetermineCharacterLine(drawStart, gen);
                    int endLine = startLine;
                    drawEnd = GetLineEndPosition(gen, endLine);
                    height -= lines[endLine].height;
                    while (true)
                    {
                        if (endLine < lines.Count - 1)
                        {
                            endLine++;
                            if (height < lines[endLine].height)
                                break;
                            drawEnd = GetLineEndPosition(gen, endLine);
                            height -= lines[endLine].height;
                        }
                        else if (startLine > 0)
                        {
                            startLine--;
                            if (height < lines[startLine].height)
                                break;
                            drawStart = GetLineStartPosition(gen, startLine);
                            height -= lines[startLine].height;
                        }
                        else
                            break;
                    }
                }
            }
            else
            {
                float width = extents.x;
                var characters = gen.characters;

                if (drawEnd <= caretPos)
                {
                    drawEnd = Mathf.Min(caretPos, gen.characterCountVisible);
                    drawStart = 0;
                    for (int i = drawEnd; i > 0; --i)
                    {
                        width -= characters[i - 1].charWidth;
                        if (width < 0)
                        {
                            drawStart = i;
                            break;
                        }
                    }
                }
                else
                {
                    if (drawStart > caretPos)
                        drawStart = caretPos;

                    drawEnd = gen.characterCountVisible;
                    for (int i = drawStart; i < gen.characterCountVisible; ++i)
                    {
                        width -= characters[i].charWidth;
                        if (width < 0)
                        {
                            drawEnd = i;
                            break;
                        }
                    }
                }
            }
        }

        private void MarkGeometryAsDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || UnityEditor.PrefabUtility.GetPrefabObject(gameObject) != null)
                return;
#endif

            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void Rebuild(CanvasUpdate update)
        {
            switch (update)
            {
                case CanvasUpdate.LatePreRender:
                    UpdateGeometry();
                    break;
            }
        }

        private void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            // No need to draw a cursor on mobile as its handled by the devices keyboard.
            if (!shouldHideMobileInput)
                return;

            if (m_CachedInputRenderer == null && m_TextComponent != null)
            {
                GameObject go = new GameObject(transform.name + " Input Caret");
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(m_TextComponent.transform.parent);
                go.transform.SetAsFirstSibling();
                go.layer = gameObject.layer;

                caretRectTrans = go.AddComponent<RectTransform>();
                m_CachedInputRenderer = go.AddComponent<CanvasRenderer>();
                m_CachedInputRenderer.SetMaterial(Graphic.defaultGraphicMaterial, null);

                // Needed as if any layout is present we want the caret to always be the same as the text area.
                go.AddComponent<LayoutElement>().ignoreLayout = true;

                AssignPositioningIfNeeded();
            }

            if (m_CachedInputRenderer == null)
                return;

            OnFillVBO(m_Vbo);

            if (m_Vbo.Count == 0)
                m_CachedInputRenderer.SetVertices(null, 0);
            else
                m_CachedInputRenderer.SetVertices(m_Vbo.ToArray(), m_Vbo.Count);

            m_Vbo.Clear();
        }

        private void AssignPositioningIfNeeded()
        {
            if (m_TextComponent != null && caretRectTrans != null &&
                (caretRectTrans.localPosition != m_TextComponent.rectTransform.localPosition ||
                 caretRectTrans.localRotation != m_TextComponent.rectTransform.localRotation ||
                 caretRectTrans.localScale != m_TextComponent.rectTransform.localScale ||
                 caretRectTrans.anchorMin != m_TextComponent.rectTransform.anchorMin ||
                 caretRectTrans.anchorMax != m_TextComponent.rectTransform.anchorMax ||
                 caretRectTrans.anchoredPosition != m_TextComponent.rectTransform.anchoredPosition ||
                 caretRectTrans.sizeDelta != m_TextComponent.rectTransform.sizeDelta ||
                 caretRectTrans.pivot != m_TextComponent.rectTransform.pivot))
            {
                caretRectTrans.localPosition = m_TextComponent.rectTransform.localPosition;
                caretRectTrans.localRotation = m_TextComponent.rectTransform.localRotation;
                caretRectTrans.localScale = m_TextComponent.rectTransform.localScale;
                caretRectTrans.anchorMin = m_TextComponent.rectTransform.anchorMin;
                caretRectTrans.anchorMax = m_TextComponent.rectTransform.anchorMax;
                caretRectTrans.anchoredPosition = m_TextComponent.rectTransform.anchoredPosition;
                caretRectTrans.sizeDelta = m_TextComponent.rectTransform.sizeDelta;
                caretRectTrans.pivot = m_TextComponent.rectTransform.pivot;
            }
        }

        private void OnFillVBO(List<UIVertex> vbo)
        {
            if (!isFocused)
                return;

            Rect inputRect = m_TextComponent.rectTransform.rect;
            Vector2 extents = inputRect.size;

            // get the text alignment anchor point for the text in local space
            Vector2 textAnchorPivot = Text.GetTextAnchorPivot(m_TextComponent.alignment);
            Vector2 refPoint = Vector2.zero;
            refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
            refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

            // Ajust the anchor point in screen space
            Vector2 roundedRefPoint = m_TextComponent.PixelAdjustPoint(refPoint);

            // Determine fraction of pixel to offset text mesh.
            // This is the rounding in screen space, plus the fraction of a pixel the text anchor pivot is from the corner of the text mesh.
            Vector2 roundingOffset = roundedRefPoint - refPoint + Vector2.Scale(extents, textAnchorPivot);
            roundingOffset.x = roundingOffset.x - Mathf.Floor(0.5f + roundingOffset.x);
            roundingOffset.y = roundingOffset.y - Mathf.Floor(0.5f + roundingOffset.y);

            if (!hasSelection)
                GenerateCursor(vbo, roundingOffset);
            else
                GenerateHightlight(vbo, roundingOffset);
        }

        private void GenerateCursor(List<UIVertex> vbo, Vector2 roundingOffset)
        {
            if (!m_CaretVisible)
                return;

            if (m_CursorVerts == null)
            {
                CreateCursorVerts();
            }

            float width = 1f;
            float height = m_TextComponent.fontSize;
            int adjustedPos = Mathf.Max(0, caretPosition - m_DrawStart);
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            if (gen == null)
                return;

            if (m_TextComponent.resizeTextForBestFit)
                height = gen.fontSizeUsedForBestFit / m_TextComponent.pixelsPerUnit;

            Vector2 startPosition = Vector2.zero;

            // Calculate startPosition.x
            if (gen.characterCountVisible + 1 > adjustedPos || adjustedPos == 0)
            {
                UICharInfo cursorChar = gen.characters[adjustedPos];
                startPosition.x = cursorChar.cursorPos.x;
            }
            startPosition.x /= m_TextComponent.pixelsPerUnit;

            // TODO: Only clamp when Text uses horizontal word wrap.
            if (startPosition.x > m_TextComponent.rectTransform.rect.xMax)
                startPosition.x = m_TextComponent.rectTransform.rect.xMax;

            // Calculate startPosition.y
            int characterLine = DetermineCharacterLine(adjustedPos, gen);
            float lineHeights = SumLineHeights(characterLine, gen);
            startPosition.y = m_TextComponent.rectTransform.rect.yMax - lineHeights / m_TextComponent.pixelsPerUnit;

            m_CursorVerts[0].position = new Vector3(startPosition.x, startPosition.y - height, 0.0f);
            m_CursorVerts[1].position = new Vector3(startPosition.x + width, startPosition.y - height, 0.0f);
            m_CursorVerts[2].position = new Vector3(startPosition.x + width, startPosition.y, 0.0f);
            m_CursorVerts[3].position = new Vector3(startPosition.x, startPosition.y, 0.0f);

            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < m_CursorVerts.Length; i++)
                {
                    UIVertex uiv = m_CursorVerts[i];
                    uiv.position.x += roundingOffset.x;
                    uiv.position.y += roundingOffset.y;
                    vbo.Add(uiv);
                }
            }
            else
            {
                for (int i = 0; i < m_CursorVerts.Length; i++)
                {
                    vbo.Add(m_CursorVerts[i]);
                }
            }

            startPosition.y = Screen.height - startPosition.y;
            Input.compositionCursorPos = startPosition;
        }

        private void CreateCursorVerts()
        {
            m_CursorVerts = new UIVertex[4];

            for (int i = 0; i < m_CursorVerts.Length; i++)
            {
                m_CursorVerts[i] = UIVertex.simpleVert;
                m_CursorVerts[i].color = m_TextComponent.color;
                m_CursorVerts[i].uv0 = Vector2.zero;
            }
        }

        private float SumLineHeights(int endLine, TextGenerator generator)
        {
            float height = 0.0f;
            for (int i = 0; i < endLine; ++i)
            {
                height += generator.lines[i].height;
            }

            return height;
        }

        private void GenerateHightlight(List<UIVertex> vbo, Vector2 roundingOffset)
        {
            int startChar = Mathf.Max(0, caretPosition - m_DrawStart);
            int endChar = Mathf.Max(0, caretSelectPos - m_DrawStart);

            // Ensure pos is always less then selPos to make the code simpler
            if (startChar > endChar)
            {
                int temp = startChar;
                startChar = endChar;
                endChar = temp;
            }

            endChar -= 1;
            TextGenerator gen = m_TextComponent.cachedTextGenerator;

            int currentLineIndex = DetermineCharacterLine(startChar, gen);
            float height = m_TextComponent.fontSize;

            if (m_TextComponent.resizeTextForBestFit)
                height = gen.fontSizeUsedForBestFit / m_TextComponent.pixelsPerUnit;

            if (cachedInputTextGenerator != null && cachedInputTextGenerator.lines.Count > 0)
            {
                // TODO: deal with multiple lines with different line heights.
                height = cachedInputTextGenerator.lines[0].height;
            }

            if (m_TextComponent.resizeTextForBestFit && cachedInputTextGenerator != null)
            {
                height = cachedInputTextGenerator.fontSizeUsedForBestFit;
            }

            int nextLineStartIdx = GetLineEndPosition(gen, currentLineIndex);

            UIVertex vert = UIVertex.simpleVert;
            vert.uv0 = Vector2.zero;
            vert.color = selectionColor;

            int currentChar = startChar;
            while (currentChar <= endChar && currentChar < gen.characterCountVisible)
            {
                if (currentChar + 1 == nextLineStartIdx || currentChar == endChar)
                {
                    UICharInfo startCharInfo = gen.characters[startChar];
                    UICharInfo endCharInfo = gen.characters[currentChar];
                    float lineHeights = SumLineHeights(currentLineIndex, gen);
                    Vector2 startPosition = new Vector2(startCharInfo.cursorPos.x / m_TextComponent.pixelsPerUnit, m_TextComponent.rectTransform.rect.yMax - (lineHeights / m_TextComponent.pixelsPerUnit));
                    Vector2 endPosition = new Vector2((endCharInfo.cursorPos.x + endCharInfo.charWidth) / m_TextComponent.pixelsPerUnit, startPosition.y - height / m_TextComponent.pixelsPerUnit);

                    // Checking xMin as well due to text generator not setting possition if char is not rendered.
                    if (endPosition.x > m_TextComponent.rectTransform.rect.xMax || endPosition.x < m_TextComponent.rectTransform.rect.xMin)
                        endPosition.x = m_TextComponent.rectTransform.rect.xMax;

                    vert.position = new Vector3(startPosition.x, endPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.Add(vert);

                    vert.position = new Vector3(endPosition.x, endPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.Add(vert);

                    vert.position = new Vector3(endPosition.x, startPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.Add(vert);

                    vert.position = new Vector3(startPosition.x, startPosition.y, 0.0f) + (Vector3)roundingOffset;
                    vbo.Add(vert);

                    startChar = currentChar + 1;
                    currentLineIndex++;

                    nextLineStartIdx = GetLineEndPosition(gen, currentLineIndex);
                }
                currentChar++;
            }
        }

        /// <summary>
        /// Validate the specified input.
        /// </summary>

        protected char Validate(string text, int pos, char ch)
        {
            // Validation is disabled
            if (characterValidation == CharacterValidation.None || !enabled)
                return ch;

            if (characterValidation == CharacterValidation.Integer || characterValidation == CharacterValidation.Decimal)
            {
                // Integer and decimal
                bool cursorBeforeDash = (pos == 0 && text.Length > 0 && text[0] == '-');
                if (!cursorBeforeDash)
                {
                    if (ch >= '0' && ch <= '9') return ch;
                    if (ch == '-' && pos == 0) return ch;
                    if (ch == '.' && characterValidation == CharacterValidation.Decimal && !text.Contains(".")) return ch;
                }
            }
            else if (characterValidation == CharacterValidation.Alphanumeric)
            {
                // All alphanumeric characters
                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
            }
            else if (characterValidation == CharacterValidation.Name)
            {
                char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';

                if (char.IsLetter(ch))
                {
                    // Space followed by a letter -- make sure it's capitalized
                    if (char.IsLower(ch) && lastChar == ' ')
                        return char.ToUpper(ch);

                    // Uppercase letters are only allowed after spaces (and apostrophes)
                    if (char.IsUpper(ch) && lastChar != ' ' && lastChar != '\'')
                        return char.ToLower(ch);

                    // If character was already in correct case, return it as-is.
                    // Also, letters that are neither upper nor lower case are always allowed.
                    return ch;
                }
                else if (ch == '\'')
                {
                    // Don't allow more than one apostrophe
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != '\'' && !text.Contains("'"))
                        return ch;
                }
                else if (ch == ' ')
                {
                    // Don't allow more than one space in a row
                    if (lastChar != ' ' && lastChar != '\'' && nextChar != ' ' && nextChar != '\'')
                        return ch;
                }
            }
            else if (characterValidation == CharacterValidation.EmailAddress)
            {
                // From StackOverflow about allowed characters in email addresses:
                // Uppercase and lowercase English letters (a-z, A-Z)
                // Digits 0 to 9
                // Characters ! # $ % & ' * + - / = ? ^ _ ` { | } ~
                // Character . (dot, period, full stop) provided that it is not the first or last character,
                // and provided also that it does not appear two or more times consecutively.

                if (ch >= 'A' && ch <= 'Z') return ch;
                if (ch >= 'a' && ch <= 'z') return ch;
                if (ch >= '0' && ch <= '9') return ch;
                if (ch == '@' && text.IndexOf('@') == -1) return ch;
                if (kEmailSpecialCharacters.IndexOf(ch) != -1) return ch;
                if (ch == '.')
                {
                    char lastChar = (text.Length > 0) ? text[Mathf.Clamp(pos, 0, text.Length - 1)] : ' ';
                    char nextChar = (text.Length > 0) ? text[Mathf.Clamp(pos + 1, 0, text.Length - 1)] : '\n';
                    if (lastChar != '.' && nextChar != '.')
                        return ch;
                }
            }
            return (char)0;
        }

        public void ActivateInputField()
        {
            if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable())
                return;

            if (isFocused)
            {
                if (m_Keyboard != null && !m_Keyboard.active)
                {
                    m_Keyboard.active = true;
                    m_Keyboard.text = m_Text;
                }
            }

            m_ShouldActivateNextUpdate = true;
        }

        private void ActivateInputFieldInternal()
        {
            if (EventSystem.current.currentSelectedGameObject != gameObject)
                EventSystem.current.SetSelectedGameObject(gameObject);

            if (TouchScreenKeyboard.isSupported)
            {
                if (Input.touchSupported)
                {
                    TouchScreenKeyboard.hideInput = shouldHideMobileInput;
                }

                m_Keyboard = (inputType == InputType.Password) ?
                    TouchScreenKeyboard.Open(m_Text, keyboardType, false, multiLine, true) :
                    TouchScreenKeyboard.Open(m_Text, keyboardType, inputType == InputType.AutoCorrect, multiLine);
            }
            else
            {
                Input.imeCompositionMode = IMECompositionMode.On;
                OnFocus();
            }

            m_AllowInput = true;
            m_OriginalText = text;
            m_WasCanceled = false;
            SetCaretVisible();
            UpdateLabel();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            ActivateInputField();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            ActivateInputField();
        }

        public void DeactivateInputField()
        {
            // Not activated do nothing.
            if (!m_AllowInput)
                return;

            m_HasDoneFocusTransition = false;
            m_AllowInput = false;

            if (m_TextComponent != null && IsActive() && IsInteractable())
            {
                if (m_WasCanceled)
                    text = m_OriginalText;

                if (m_Keyboard != null)
                {
                    m_Keyboard.active = false;
                    m_Keyboard = null;
                }

                m_CaretPosition = m_CaretSelectPosition = 0;

                SendOnSubmit();

                Input.imeCompositionMode = IMECompositionMode.Auto;
            }

            MarkGeometryAsDirty();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            DeactivateInputField();
            base.OnDeselect(eventData);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (!isFocused)
                m_ShouldActivateNextUpdate = true;
        }

        private void EnforceContentType()
        {
            switch (contentType)
            {
                case ContentType.Standard:
                {
                    // Don't enforce line type for this content type.
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.Default;
                    m_CharacterValidation = CharacterValidation.None;
                    return;
                }
                case ContentType.Autocorrected:
                {
                    // Don't enforce line type for this content type.
                    m_InputType = InputType.AutoCorrect;
                    m_KeyboardType = TouchScreenKeyboardType.Default;
                    m_CharacterValidation = CharacterValidation.None;
                    return;
                }
                case ContentType.IntegerNumber:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                    m_CharacterValidation = CharacterValidation.Integer;
                    return;
                }
                case ContentType.DecimalNumber:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation;
                    m_CharacterValidation = CharacterValidation.Decimal;
                    return;
                }
                case ContentType.Alphanumeric:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.ASCIICapable;
                    m_CharacterValidation = CharacterValidation.Alphanumeric;
                    return;
                }
                case ContentType.Name:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.Default;
                    m_CharacterValidation = CharacterValidation.Name;
                    return;
                }
                case ContentType.EmailAddress:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Standard;
                    m_KeyboardType = TouchScreenKeyboardType.EmailAddress;
                    m_CharacterValidation = CharacterValidation.EmailAddress;
                    return;
                }
                case ContentType.Password:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Password;
                    m_KeyboardType = TouchScreenKeyboardType.Default;
                    m_CharacterValidation = CharacterValidation.None;
                    return;
                }
                case ContentType.Pin:
                {
                    m_LineType = LineType.SingleLine;
                    m_InputType = InputType.Password;
                    m_KeyboardType = TouchScreenKeyboardType.NumberPad;
                    m_CharacterValidation = CharacterValidation.Integer;
                    return;
                }
                default:
                {
                    // Includes Custom type. Nothing should be enforced.
                    return;
                }
            }
        }

        void SetToCustomIfContentTypeIsNot(params ContentType[] allowedContentTypes)
        {
            if (contentType == ContentType.Custom)
                return;

            for (int i = 0; i < allowedContentTypes.Length; i++)
                if (contentType == allowedContentTypes[i])
                    return;

            contentType = ContentType.Custom;
        }

        void SetToCustom()
        {
            if (contentType == ContentType.Custom)
                return;

            contentType = ContentType.Custom;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (m_HasDoneFocusTransition)
                state = SelectionState.Highlighted;
            else if (state == SelectionState.Pressed)
                m_HasDoneFocusTransition = true;

            base.DoStateTransition(state, instant);
        }
    }
}
