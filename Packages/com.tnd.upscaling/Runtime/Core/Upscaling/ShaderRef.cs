using UnityEngine;

namespace TND.Upscaling.Framework
{
    public class ShaderRef
    {
        private readonly string _shaderResourceName;

        private Shader _shader;
        private int _refCount;
        
        internal ShaderRef(string shaderResourceName)
        {
            _shaderResourceName = shaderResourceName;
        }

        internal Shader Acquire()
        {
            if (_shader == null)
            {
                _shader = Resources.Load<Shader>(_shaderResourceName);
                if (_shader == null)
                    return null;
            }

            _refCount++;
            return _shader;
        }

        internal void Release()
        {
            if (_shader == null)
            {
                _refCount = 0;
                return;
            }

            if (--_refCount <= 0)
            {
                Resources.UnloadAsset(_shader);
                _shader = null;
                _refCount = 0;
            }
        }
    }
}
