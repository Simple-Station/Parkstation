﻿using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Prototype to store chat typing indicator visuals.
/// </summary>
[Prototype("typingIndicator")]
public sealed class TypingIndicatorPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("spritePath")]
    public ResPath SpritePath = new("/Textures/Effects/speech.rsi");

    [DataField("typingState", required: true)]
    public string TypingState = default!;

    // Corvax-TypingIndicator-Start
    [DataField("idleState", required: true)]
    public string IdleState = default!;
    // Corvax-TypingIndicator-End

    [DataField("offset")]
    public Vector2 Offset = new(0.5f, 0.5f);

    [DataField("shader")]
    public string Shader = "unshaded";
}
