using System;
using UnityEngine;

public static class Utility
{
    /// <summary>
    /// Temporarily Activate a GameObject.
    /// Disposing calls SetActive(false) on that GameObject.
    /// Either do this manually, or have it automatically handled with using().
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns>IDisposable</returns>
    public static IDisposable TemporarilyActivate(GameObject gameObject)
    {
        return new TemporarilyActiveObject(gameObject);
    }

    public readonly struct TemporarilyActiveObject : IDisposable
    {
        private readonly GameObject _gameObject;
        public readonly GameObject GameObject => _gameObject;

        public TemporarilyActiveObject(GameObject gameObject)
        {
            _gameObject = gameObject;
            _gameObject.SetActive(true);
        }

        public readonly void Dispose()
        {
            _gameObject.SetActive(false);
        }
    }
}
