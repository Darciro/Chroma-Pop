using UnityEngine;

namespace ChromaPop
{
    /// <summary>
    /// Utility class for color conversions.
    /// </summary>
    public static class ColorUtility
    {
        public static Color GetColorFromEnum(BalloonColorEnum balloonColor)
        {
            return balloonColor switch
            {
                BalloonColorEnum.Blue => new Color(.3f, 0.8f, 1f),
                BalloonColorEnum.Green => new Color(.68f, 0.85f, 0),
                BalloonColorEnum.Orange => new Color(1f, 0.5f, 0f),
                BalloonColorEnum.Pink => new Color(1f, 0.75f, 0.8f),
                BalloonColorEnum.Purple => new Color(0.5f, 0f, 0.5f),
                BalloonColorEnum.Red => new Color(0.83f, 0, 0),
                BalloonColorEnum.Yellow => new Color(1f, .83f, .16f),
                _ => Color.white
            };
        }
    }
}
