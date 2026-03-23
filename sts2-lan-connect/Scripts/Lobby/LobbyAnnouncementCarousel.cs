using System;
using System.Collections.Generic;
using Godot;

namespace Sts2LanConnect.Scripts;

internal sealed partial class LobbyAnnouncementCarousel : Control
{
    private static readonly Color BaseBackgroundColor = new(0.09f, 0.055f, 0.032f, 0.96f);
    private static readonly Color TextStrongColor = new(0.93f, 0.9f, 0.86f, 1f);
    private static readonly Color TextMutedColor = new(0.73f, 0.67f, 0.61f, 1f);
    private static readonly Color AccentColor = new(0.954f, 0.431f, 0.203f, 1f);
    private static readonly Color AccentBrightColor = new(0.996f, 0.58f, 0.274f, 1f);

    private readonly List<LobbyAnnouncementItem> _announcements = new();

    private PanelContainer? _frame;
    private SmoothGradientControl? _backgroundGradient;
    private ColorRect? _topGlow;
    private PanelContainer? _iconOrb;
    private Label? _iconLabel;
    private Label? _titleLabel;
    private Label? _dateLabel;
    private Label? _bodyLabel;
    private Button? _previousButton;
    private Button? _nextButton;
    private HBoxContainer? _indicatorContainer;
    private Label? _counterLabel;
    private Control? _progressTrack;
    private ColorRect? _progressFill;

    private bool _compactMode;
    private bool _paused;
    private int _currentIndex;
    private double _elapsed;

    public LobbyAnnouncementCarousel()
    {
        MouseFilter = MouseFilterEnum.Stop;
        ProcessMode = ProcessModeEnum.Always;
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        CustomMinimumSize = new Vector2(0f, 94f);
        BuildUi();
    }

    public double AutoAdvanceSeconds { get; set; } = 6d;

    public override void _Process(double delta)
    {
        if (!IsVisibleInTree() || _announcements.Count <= 1)
        {
            UpdateProgress();
            return;
        }

        if (_paused)
        {
            UpdateProgress();
            return;
        }

        _elapsed += delta;
        if (_elapsed >= AutoAdvanceSeconds)
        {
            Advance(1);
            return;
        }

        UpdateProgress();
    }

    public void SetAnnouncements(IReadOnlyList<LobbyAnnouncementItem> announcements)
    {
        if (AreAnnouncementsEquivalent(announcements))
        {
            RefreshCurrentAnnouncement();
            return;
        }

        _announcements.Clear();
        foreach (LobbyAnnouncementItem announcement in announcements)
        {
            _announcements.Add(new LobbyAnnouncementItem
            {
                Id = announcement.Id,
                Type = announcement.Type,
                Title = announcement.Title,
                DateLabel = announcement.DateLabel,
                Body = announcement.Body,
                Enabled = announcement.Enabled,
            });
        }

        _currentIndex = _announcements.Count == 0
            ? 0
            : Math.Clamp(_currentIndex, 0, _announcements.Count - 1);
        _elapsed = 0d;
        RebuildIndicators();
        RefreshCurrentAnnouncement();
    }

    public void SetCompactMode(bool compactMode)
    {
        if (_compactMode == compactMode)
        {
            return;
        }

        _compactMode = compactMode;
        UpdateControlVisibility();
        RefreshCurrentAnnouncement();
    }

    private void BuildUi()
    {
        _frame = CreatePanel(BaseBackgroundColor, new Color(AccentColor, 0.18f), radius: 18, borderWidth: 1, padding: 0, shadowSize: 4, shadowColor: new Color(AccentColor, 0.02f));
        _frame.SetAnchorsPreset(LayoutPreset.FullRect);
        _frame.MouseFilter = MouseFilterEnum.Stop;
        _frame.Connect(Control.SignalName.MouseEntered, Callable.From(() => _paused = true));
        _frame.Connect(Control.SignalName.MouseExited, Callable.From(() => _paused = false));
        AddChild(_frame);

        Control chrome = new()
        {
            MouseFilter = MouseFilterEnum.Ignore,
            ClipContents = true
        };
        chrome.SetAnchorsPreset(LayoutPreset.FullRect);
        _frame.AddChild(chrome);

        _backgroundGradient = new SmoothGradientControl
        {
            MouseFilter = MouseFilterEnum.Ignore
        };
        _backgroundGradient.SetAnchorsPreset(LayoutPreset.FullRect);
        chrome.AddChild(_backgroundGradient);

        _topGlow = new ColorRect
        {
            Color = new Color(AccentBrightColor, 0.045f),
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(0f, 1f)
        };
        _topGlow.SetAnchorsPreset(LayoutPreset.TopWide);
        chrome.AddChild(_topGlow);

        MarginContainer content = new();
        content.SetAnchorsPreset(LayoutPreset.FullRect);
        content.OffsetLeft = 24f;
        content.OffsetTop = 14f;
        content.OffsetRight = -24f;
        content.OffsetBottom = -12f;
        _frame.AddChild(content);

        VBoxContainer root = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        root.AddThemeConstantOverride("separation", 12);
        content.AddChild(root);

        HBoxContainer mainRow = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        mainRow.AddThemeConstantOverride("separation", 18);
        root.AddChild(mainRow);

        _previousButton = CreateNavButton("‹", () => Advance(-1));
        mainRow.AddChild(_previousButton);

        HBoxContainer infoRow = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        infoRow.AddThemeConstantOverride("separation", 14);
        mainRow.AddChild(infoRow);

        _iconOrb = CreatePanel(new Color(0.055f, 0.04f, 0.028f, 0.98f), new Color(AccentColor, 0.14f), radius: 999, borderWidth: 1, padding: 0);
        _iconOrb.CustomMinimumSize = new Vector2(48f, 48f);
        _iconOrb.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        infoRow.AddChild(_iconOrb);

        CenterContainer iconCenter = new();
        _iconOrb.AddChild(iconCenter);

        _iconLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _iconLabel.AddThemeColorOverride("font_color", AccentColor);
        _iconLabel.AddThemeFontSizeOverride("font_size", 22);
        iconCenter.AddChild(_iconLabel);

        VBoxContainer textGroup = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        textGroup.AddThemeConstantOverride("separation", 4);
        infoRow.AddChild(textGroup);

        HBoxContainer headingRow = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        headingRow.AddThemeConstantOverride("separation", 8);
        textGroup.AddChild(headingRow);

        _titleLabel = new Label
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _titleLabel.AddThemeColorOverride("font_color", TextStrongColor);
        _titleLabel.AddThemeFontSizeOverride("font_size", 22);
        headingRow.AddChild(_titleLabel);

        _dateLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        _dateLabel.AddThemeColorOverride("font_color", TextMutedColor);
        _dateLabel.AddThemeFontSizeOverride("font_size", 15);
        headingRow.AddChild(_dateLabel);

        _bodyLabel = new Label
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _bodyLabel.AddThemeColorOverride("font_color", TextMutedColor);
        _bodyLabel.AddThemeFontSizeOverride("font_size", 16);
        textGroup.AddChild(_bodyLabel);

        HBoxContainer controls = new()
        {
            SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        controls.AddThemeConstantOverride("separation", 14);
        mainRow.AddChild(controls);

        _indicatorContainer = new HBoxContainer
        {
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        _indicatorContainer.AddThemeConstantOverride("separation", 8);
        controls.AddChild(_indicatorContainer);

        _counterLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        _counterLabel.AddThemeColorOverride("font_color", AccentColor);
        _counterLabel.AddThemeFontSizeOverride("font_size", 15);
        controls.AddChild(_counterLabel);

        _nextButton = CreateNavButton("›", () => Advance(1));
        controls.AddChild(_nextButton);

        _progressTrack = new Control
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0f, 3f),
            ClipContents = true,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _progressTrack.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.16f, 0.095f, 0.05f, 0.32f), new Color(AccentColor, 0.04f), radius: 999, borderWidth: 0, padding: 0));
        _progressTrack.Connect(Control.SignalName.Resized, Callable.From(UpdateProgress));
        root.AddChild(_progressTrack);

        _progressFill = new ColorRect
        {
            Color = new Color(AccentColor, 0.92f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        _progressFill.SetAnchorsPreset(LayoutPreset.LeftWide);
        _progressTrack.AddChild(_progressFill);

        UpdateControlVisibility();
    }

    private void Advance(int direction)
    {
        if (_announcements.Count == 0)
        {
            return;
        }

        _currentIndex = (_currentIndex + direction + _announcements.Count) % _announcements.Count;
        _elapsed = 0d;
        RefreshCurrentAnnouncement();
    }

    private void JumpToAnnouncement(int index)
    {
        if (index < 0 || index >= _announcements.Count || index == _currentIndex)
        {
            return;
        }

        _currentIndex = index;
        _elapsed = 0d;
        RefreshCurrentAnnouncement();
    }

    private void RefreshCurrentAnnouncement()
    {
        LobbyAnnouncementItem current = GetCurrentAnnouncement();
        AnnouncementVisualStyle style = GetVisualStyle(current.Type);

        if (_frame != null)
        {
            _frame.AddThemeStyleboxOverride("panel", CreatePanelStyle(BaseBackgroundColor, style.Border, radius: 18, borderWidth: 1, padding: 0, shadowSize: 4, shadowColor: new Color(style.Border, 0.018f)));
        }

        _backgroundGradient?.SetColors(style.LeftGlow, style.CenterGlow, style.RightGlow);

        if (_topGlow != null)
        {
            _topGlow.Color = style.TopGlow;
        }

        if (_iconOrb != null)
        {
            _iconOrb.AddThemeStyleboxOverride("panel", CreatePanelStyle(new Color(0.055f, 0.04f, 0.028f, 0.98f), new Color(style.Border, 0.1f), radius: 999, borderWidth: 1, padding: 0));
        }

        if (_iconLabel != null)
        {
            _iconLabel.Text = style.IconText;
            _iconLabel.AddThemeColorOverride("font_color", style.Accent);
        }

        if (_titleLabel != null)
        {
            _titleLabel.Text = NormalizeText(string.IsNullOrWhiteSpace(current.Title) ? "暂无公告" : current.Title.Trim());
        }

        if (_dateLabel != null)
        {
            _dateLabel.Text = NormalizeText(current.DateLabel?.Trim() ?? string.Empty);
            _dateLabel.Visible = !string.IsNullOrWhiteSpace(_dateLabel.Text);
        }

        if (_bodyLabel != null)
        {
            _bodyLabel.Text = NormalizeText(string.IsNullOrWhiteSpace(current.Body) ? "浏览房间列表，或稍后刷新查看最新公告。" : current.Body.Trim());
        }

        if (_counterLabel != null)
        {
            int total = Math.Max(_announcements.Count, 1);
            _counterLabel.Text = $"{Math.Min(_currentIndex + 1, total)}/{total}";
        }

        UpdateIndicatorSelection(style);
        UpdateControlVisibility();
        UpdateProgress();
    }

    private void RebuildIndicators()
    {
        if (_indicatorContainer == null)
        {
            return;
        }

        foreach (Node child in _indicatorContainer.GetChildren())
        {
            child.QueueFree();
        }

        for (int index = 0; index < _announcements.Count; index++)
        {
            int selectedIndex = index;
            Button dot = new()
            {
                Text = string.Empty,
                CustomMinimumSize = new Vector2(8f, 8f),
                FocusMode = FocusModeEnum.None,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            ApplyIndicatorStyle(dot, new Color(0.36f, 0.31f, 0.27f, 0.74f), active: false);
            dot.Connect(Button.SignalName.Pressed, Callable.From(() => JumpToAnnouncement(selectedIndex)));
            _indicatorContainer.AddChild(dot);
        }
    }

    private void UpdateIndicatorSelection(AnnouncementVisualStyle style)
    {
        if (_indicatorContainer == null)
        {
            return;
        }

        for (int index = 0; index < _indicatorContainer.GetChildCount(); index++)
        {
            if (_indicatorContainer.GetChild(index) is not Button button)
            {
                continue;
            }

            bool active = index == _currentIndex;
            button.CustomMinimumSize = active ? new Vector2(22f, 8f) : new Vector2(8f, 8f);
            ApplyIndicatorStyle(button, active ? style.Accent : new Color(0.36f, 0.31f, 0.27f, 0.74f), active);
        }
    }

    private void UpdateControlVisibility()
    {
        bool hasMultiple = _announcements.Count > 1;
        if (_previousButton != null)
        {
            _previousButton.Visible = !_compactMode && hasMultiple;
        }

        if (_nextButton != null)
        {
            _nextButton.Visible = !_compactMode && hasMultiple;
        }

        if (_indicatorContainer != null)
        {
            _indicatorContainer.Visible = !_compactMode && hasMultiple;
        }

        if (_counterLabel != null)
        {
            _counterLabel.Visible = _compactMode && hasMultiple;
        }
    }

    private void UpdateProgress()
    {
        if (_progressTrack == null || _progressFill == null)
        {
            return;
        }

        float width = _progressTrack.Size.X;
        float progressRatio = _announcements.Count <= 1 || AutoAdvanceSeconds <= 0d
            ? 1f
            : Mathf.Clamp((float)(_elapsed / AutoAdvanceSeconds), 0f, 1f);
        _progressFill.Size = new Vector2(width * progressRatio, Math.Max(_progressTrack.Size.Y, 4f));
    }

    private bool AreAnnouncementsEquivalent(IReadOnlyList<LobbyAnnouncementItem> announcements)
    {
        if (_announcements.Count != announcements.Count)
        {
            return false;
        }

        for (int index = 0; index < announcements.Count; index++)
        {
            LobbyAnnouncementItem current = _announcements[index];
            LobbyAnnouncementItem next = announcements[index];
            if (!string.Equals(current.Id, next.Id, StringComparison.Ordinal) ||
                !string.Equals(current.Type, next.Type, StringComparison.Ordinal) ||
                !string.Equals(current.Title, next.Title, StringComparison.Ordinal) ||
                !string.Equals(current.DateLabel, next.DateLabel, StringComparison.Ordinal) ||
                !string.Equals(current.Body, next.Body, StringComparison.Ordinal) ||
                current.Enabled != next.Enabled)
            {
                return false;
            }
        }

        return true;
    }

    private static void ApplyIndicatorStyle(Button button, Color baseColor, bool active)
    {
        Color hoverColor = active ? baseColor.Lightened(0.08f) : new Color(0.46f, 0.38f, 0.32f, 0.82f);
        Color pressedColor = active ? baseColor.Darkened(0.08f) : new Color(0.46f, 0.38f, 0.32f, 0.86f);
        button.AddThemeStyleboxOverride("normal", CreatePanelStyle(baseColor, Colors.Transparent, radius: 999, borderWidth: 0, padding: 0));
        button.AddThemeStyleboxOverride("hover", CreatePanelStyle(hoverColor, Colors.Transparent, radius: 999, borderWidth: 0, padding: 0));
        button.AddThemeStyleboxOverride("pressed", CreatePanelStyle(pressedColor, Colors.Transparent, radius: 999, borderWidth: 0, padding: 0));
        button.AddThemeStyleboxOverride("focus", CreatePanelStyle(hoverColor, Colors.Transparent, radius: 999, borderWidth: 0, padding: 0));
    }

    private LobbyAnnouncementItem GetCurrentAnnouncement()
    {
        if (_announcements.Count == 0)
        {
            return new LobbyAnnouncementItem
            {
                Id = "default",
                Type = "info",
                Title = "暂无公告",
                Body = "浏览房间列表，或稍后刷新查看最新公告。",
                Enabled = true
            };
        }

        return _announcements[Math.Clamp(_currentIndex, 0, _announcements.Count - 1)];
    }

    private static Button CreateNavButton(string text, Action onPressed)
    {
        Button button = new()
        {
            Text = text,
            CustomMinimumSize = new Vector2(38f, 38f),
            FocusMode = FocusModeEnum.None,
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        button.AddThemeStyleboxOverride("normal", CreatePanelStyle(new Color(0.16f, 0.07f, 0.05f, 0.88f), Colors.Transparent, radius: 999, borderWidth: 0, padding: 8));
        button.AddThemeStyleboxOverride("hover", CreatePanelStyle(new Color(0.22f, 0.09f, 0.06f, 0.96f), Colors.Transparent, radius: 999, borderWidth: 0, padding: 8));
        button.AddThemeStyleboxOverride("pressed", CreatePanelStyle(new Color(0.26f, 0.1f, 0.06f, 0.98f), Colors.Transparent, radius: 999, borderWidth: 0, padding: 8));
        button.AddThemeStyleboxOverride("focus", CreatePanelStyle(new Color(0.22f, 0.09f, 0.06f, 0.96f), Colors.Transparent, radius: 999, borderWidth: 0, padding: 8));
        button.AddThemeColorOverride("font_color", TextStrongColor);
        button.AddThemeFontSizeOverride("font_size", 22);
        button.Connect(Button.SignalName.Pressed, Callable.From(onPressed));
        return button;
    }

    private static PanelContainer CreatePanel(Color background, Color border, int radius, int borderWidth, int padding, int shadowSize = 0, Color? shadowColor = null)
    {
        PanelContainer panel = new()
        {
            ClipContents = true
        };
        panel.AddThemeStyleboxOverride("panel", CreatePanelStyle(background, border, radius, borderWidth, padding, shadowSize, shadowColor));
        return panel;
    }

    private static StyleBoxFlat CreatePanelStyle(Color background, Color border, int radius, int borderWidth, int padding, int shadowSize = 0, Color? shadowColor = null)
    {
        StyleBoxFlat style = new()
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            CornerRadiusBottomRight = radius,
            CornerRadiusBottomLeft = radius,
            ContentMarginLeft = padding,
            ContentMarginTop = padding,
            ContentMarginRight = padding,
            ContentMarginBottom = padding
        };
        style.ShadowColor = shadowColor ?? new Color(0f, 0f, 0f, 0f);
        style.ShadowSize = shadowSize;
        style.ShadowOffset = Vector2.Zero;
        return style;
    }

    private static string NormalizeText(string text)
    {
        return LanConnectUiText.NormalizeForDisplay(text);
    }

    private static AnnouncementVisualStyle GetVisualStyle(string? type)
    {
        return type switch
        {
            "update" => new AnnouncementVisualStyle(
                "✦",
                new Color(0.46f, 0.77f, 1f, 0.96f),
                new Color(0.15f, 0.16f, 0.28f, 0.22f),
                new Color(0.13f, 0.1f, 0.22f, 0.12f),
                new Color(0.03f, 0.02f, 0.015f, 0.24f),
                new Color(0.46f, 0.77f, 1f, 0.045f),
                new Color(0.32f, 0.52f, 0.78f, 0.22f)),
            "event" => new AnnouncementVisualStyle(
                "❖",
                new Color(0.93f, 0.46f, 0.84f, 0.96f),
                new Color(0.36f, 0.12f, 0.18f, 0.24f),
                new Color(0.22f, 0.08f, 0.14f, 0.13f),
                new Color(0.03f, 0.02f, 0.015f, 0.24f),
                new Color(0.93f, 0.46f, 0.84f, 0.05f),
                new Color(0.71f, 0.22f, 0.4f, 0.22f)),
            "warning" => new AnnouncementVisualStyle(
                "!",
                new Color(0.99f, 0.53f, 0.23f, 0.98f),
                new Color(0.44f, 0.14f, 0.08f, 0.26f),
                new Color(0.28f, 0.1f, 0.05f, 0.16f),
                new Color(0.03f, 0.02f, 0.015f, 0.26f),
                new Color(0.99f, 0.53f, 0.23f, 0.055f),
                new Color(0.78f, 0.25f, 0.13f, 0.22f)),
            _ => new AnnouncementVisualStyle(
                "i",
                new Color(0.38f, 0.88f, 0.82f, 0.96f),
                new Color(0.11f, 0.26f, 0.24f, 0.2f),
                new Color(0.09f, 0.18f, 0.16f, 0.12f),
                new Color(0.03f, 0.02f, 0.015f, 0.24f),
                new Color(0.38f, 0.88f, 0.82f, 0.04f),
                new Color(0.17f, 0.58f, 0.52f, 0.18f)),
        };
    }

    private readonly record struct AnnouncementVisualStyle(
        string IconText,
        Color Accent,
        Color LeftGlow,
        Color CenterGlow,
        Color RightGlow,
        Color TopGlow,
        Color Border);

    private sealed partial class SmoothGradientControl : Control
    {
        private Color _leftColor = Colors.Transparent;
        private Color _centerColor = Colors.Transparent;
        private Color _rightColor = Colors.Transparent;

        public SmoothGradientControl()
        {
            ClipContents = true;
        }

        public void SetColors(Color leftColor, Color centerColor, Color rightColor)
        {
            _leftColor = leftColor;
            _centerColor = centerColor;
            _rightColor = rightColor;
            QueueRedraw();
        }

        public override void _Draw()
        {
            int width = Math.Max(Mathf.RoundToInt(Size.X), 1);
            float height = Math.Max(Size.Y, 1f);
            for (int x = 0; x < width; x++)
            {
                float t = width <= 1 ? 0f : x / (float)(width - 1);
                Color color = SampleGradient(t);
                DrawLine(new Vector2(x, 0f), new Vector2(x, height), color, 1f, true);
            }
        }

        private Color SampleGradient(float t)
        {
            const float leftSpan = 0.42f;
            if (t <= leftSpan)
            {
                float local = Mathf.SmoothStep(0f, 1f, t / leftSpan);
                return _leftColor.Lerp(_centerColor, local);
            }

            float tail = Mathf.Clamp((t - leftSpan) / (1f - leftSpan), 0f, 1f);
            float smoothTail = Mathf.SmoothStep(0f, 1f, tail);
            return _centerColor.Lerp(_rightColor, smoothTail);
        }
    }
}
