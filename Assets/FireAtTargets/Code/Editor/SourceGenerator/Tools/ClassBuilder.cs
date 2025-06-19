using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreedLOW.FireAtTargets.Code.Editor.SourceGenerator.Tools
{
    public class ClassInstance
    {
        private List<FieldInstance> _fields = new();
        private List<PropertyInstance> _properties = new();
        private List<MethodInstance> _methods = new();
        private List<ClassInstance> _classes = new();
        private List<string> _inheritances = new();
        private List<string> _usings = new();

        private ModifierKeyWord _modifierKeyWord = ModifierKeyWord.None;
        private AccessType _accessType = AccessType.None;

        private string _name;
        private string _namespace;
        private int _indent = 0;

        public string GetString()
        {
            int innerIndent = _indent;
            StringBuilder classStringBuilder = new StringBuilder();

            if (_usings.Any())
            {
                foreach (string usingInstance in _usings)
                {
                    classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + usingInstance + ToolConstants.SEMI_COLON);
                }

                classStringBuilder.AppendLine();
            }

            if (_namespace != null)
            {
                classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + _namespace + ToolConstants.OPEN_BRACER);
                innerIndent++;
            }

            classStringBuilder.Append(IndentGenerator.GetIndent(innerIndent));

            if (_accessType != AccessType.None)
            {
                classStringBuilder.Append(_accessType.ToString().ToLower() + " ");
            }

            if (_modifierKeyWord != ModifierKeyWord.None)
            {
                classStringBuilder.Append(_modifierKeyWord.ToString().ToLower() + " ");
            }

            classStringBuilder.Append("class " + _name);

            if (_inheritances.Any())
            {
                classStringBuilder.Append(ToolConstants.COLON);
                for (int index = 0; index < _inheritances.Count; index++)
                {
                    if (index != 0)
                    {
                        classStringBuilder.Append(ToolConstants.COMA);
                    }

                    classStringBuilder.Append(_inheritances[index]);
                }
            }

            classStringBuilder.AppendLine(ToolConstants.OPEN_BRACER);
            innerIndent++;

            foreach (FieldInstance fieldInstance in _fields)
            {
                classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + fieldInstance.GetString());
            }

            foreach (PropertyInstance propertyInstance in _properties)
            {
                classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + propertyInstance.GetString());
            }

            foreach (MethodInstance methodInstance in _methods)
            {
                classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + methodInstance.GetString());
            }

            foreach (ClassInstance classInstance in _classes)
            {
                classStringBuilder.AppendLine(classInstance.Copy().SetIndent(innerIndent).GetString());
            }

            innerIndent--;
            classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + ToolConstants.CLOSE_BRACER);
            innerIndent--;

            if (_namespace != null)
            {
                classStringBuilder.AppendLine(IndentGenerator.GetIndent(innerIndent) + ToolConstants.CLOSE_BRACER);
            }

            return classStringBuilder.ToString();
        }

        private ClassInstance Copy()
        {
            return new ClassInstance
            {
                _name = _name,
                _indent = _indent,
                _namespace = _namespace,
                _accessType = _accessType,
                _modifierKeyWord = _modifierKeyWord,
                _usings = new List<string>(_usings),
                _inheritances = new List<string>(_inheritances),
                _fields = new List<FieldInstance>(_fields.Select(field => field.Copy())),
                _properties = new List<PropertyInstance>(_properties.Select(property => property.Copy())),
                _methods = new List<MethodInstance>(_methods.Select(method => method.Copy())),
                _classes = new List<ClassInstance>(_classes.Select(oldClass => oldClass.Copy()))
            };
        }

        public ClassInstance SetIndent(int indent)
        {
            _indent = indent;
            return this;
        }

        public ClassInstance AddUsing(string usingInstance)
        {
            _usings.Add($"using {usingInstance}");
            return this;
        }

        public ClassInstance AddNamespace(string namespaceInstance)
        {
            _namespace = $"namespace {namespaceInstance}";
            return this;
        }

        public ClassInstance SetAccessType(AccessType accessType)
        {
            _accessType = accessType;
            return this;
        }

        public ClassInstance SetPublic()
        {
            _accessType = AccessType.Public;
            return this;
        }

        public ClassInstance SetPrivate()
        {
            _accessType = AccessType.Private;
            return this;
        }

        public ClassInstance SetProtected()
        {
            _accessType = AccessType.Protected;
            return this;
        }

        public ClassInstance SetInternal()
        {
            _accessType = AccessType.Internal;
            return this;
        }

        public ClassInstance SetModifierKeyWord(ModifierKeyWord modifierKeyWord)
        {
            _modifierKeyWord = modifierKeyWord;
            return this;
        }

        public ClassInstance SetStatic()
        {
            _modifierKeyWord = ModifierKeyWord.Static;
            return this;
        }

        public ClassInstance SetConst()
        {
            _modifierKeyWord = ModifierKeyWord.Const;
            return this;
        }

        public ClassInstance SetAbstract()
        {
            _modifierKeyWord = ModifierKeyWord.Abstract;
            return this;
        }

        public ClassInstance SetVirtual()
        {
            _modifierKeyWord = ModifierKeyWord.Virtual;
            return this;
        }

        public ClassInstance SetOverride()
        {
            _modifierKeyWord = ModifierKeyWord.Override;
            return this;
        }

        public ClassInstance SetName(string name)
        {
            _name = name;
            return this;
        }

        public ClassInstance AddInheritance(string inheritance)
        {
            _inheritances.Add(inheritance);
            return this;
        }

        public ClassInstance AddField(FieldInstance fieldInstance)
        {
            _fields.Add(fieldInstance);
            return this;
        }

        public ClassInstance AddProperty(PropertyInstance propertyInstance)
        {
            _properties.Add(propertyInstance);
            return this;
        }

        public ClassInstance AddMethod(MethodInstance methodInstance)
        {
            _methods.Add(methodInstance);
            return this;
        }

        public ClassInstance AddClass(ClassInstance classInstance)
        {
            _classes.Add(classInstance);
            return this;
        }
    }
}