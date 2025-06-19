using System;
using System.Collections.Generic;
using System.Text;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Tools
{
    public class MethodInstance
    {
        private AccessType _accessType = AccessType.None;
        private ModifierKeyWord _modifierKeyWord = ModifierKeyWord.None;
        private List<string> _parameters = new();
        private Type _returnType = typeof(void);
        private string _name;
        private string _methodBody;

        public string GetString()
        {
            StringBuilder methodStringBuilder = new StringBuilder();

            if (_accessType != AccessType.None)
            {
                methodStringBuilder.Append(_accessType.ToString().ToLower() + " ");
            }

            if (_modifierKeyWord != ModifierKeyWord.None)
            {
                methodStringBuilder.Append(_modifierKeyWord.ToString().ToLower() + " ");
            }
        
            methodStringBuilder.Append(_returnType + " ");
            methodStringBuilder.Append(_name);
        
            methodStringBuilder.Append(ToolConstants.OPEN_BRACKET);
            methodStringBuilder.Append(string.Join(ToolConstants.COMA, _parameters));
            methodStringBuilder.Append(ToolConstants.CLOSE_BRACKET);

            if (string.IsNullOrEmpty(_methodBody) is false)
            {
                methodStringBuilder.Append(ToolConstants.OPEN_BRACER + "\n" + _methodBody + "\n" + ToolConstants.CLOSE_BRACER);
            }
            else
            {
                methodStringBuilder.Append(ToolConstants.OPEN_BRACER + " " + ToolConstants.CLOSE_BRACER);
            }

            return methodStringBuilder.ToString();
        }

        public MethodInstance Copy()
        {
            return new MethodInstance
            {
                _accessType = _accessType,
                _modifierKeyWord = _modifierKeyWord,
                _returnType = _returnType,
                _name = _name,
                _methodBody = _methodBody,
                _parameters = _parameters
            };
        }

        public MethodInstance SetAccessType(AccessType accessType)
        {
            _accessType = accessType;
            return this;
        }

        public MethodInstance SetPublic()
        {
            _accessType = AccessType.Public;
            return this;
        }

        public MethodInstance SetPrivate()
        {
            _accessType = AccessType.Private;
            return this;
        }

        public MethodInstance SetProtected()
        {
            _accessType = AccessType.Protected;
            return this;
        }

        public MethodInstance SetInternal()
        {
            _accessType = AccessType.Internal;
            return this;
        }

        public MethodInstance SetModifierKeyWord(ModifierKeyWord modifierKeyWord)
        {
            _modifierKeyWord = modifierKeyWord;
            return this;
        }

        public MethodInstance SetStatic()
        {
            _modifierKeyWord = ModifierKeyWord.Static;
            return this;
        }

        public MethodInstance SetAbstract()
        {
            _modifierKeyWord = ModifierKeyWord.Abstract;
            return this;
        }

        public MethodInstance SetVirtual()
        {
            _modifierKeyWord = ModifierKeyWord.Virtual;
            return this;
        }

        public MethodInstance SetOverride()
        {
            _modifierKeyWord = ModifierKeyWord.Override;
            return this;
        }

        public MethodInstance SetType(Type type)
        {
            _returnType = type;
            return this;
        }

        public MethodInstance SetStringType()
        {
            _returnType = typeof(string);
            return this;
        }

        public MethodInstance SetIntType()
        {
            _returnType = typeof(int);
            return this;
        }

        public MethodInstance SetFloatType()
        {
            _returnType = typeof(float);
            return this;
        }

        public MethodInstance SetDoubleType()
        {
            _returnType = typeof(double);
            return this;
        }

        public MethodInstance SetName(string name)
        {
            _name = name;
            return this;
        }

        public MethodInstance SetMethodBody(string methodBody)
        {
            _methodBody = methodBody;
            return this;
        }

        public MethodInstance AddParameter(string parameter)
        {
            _parameters.Add(parameter);
            return this;
        }
    }
}