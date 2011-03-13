using System;
using System.Collections.Generic;
using System.IO;

namespace Lextm.SharpSnmpLib.Mib
{
    /// <summary>
    /// Base class of object registry.
    /// </summary>
    public abstract class ObjectRegistryBase : IObjectRegistry
    {
        private IObjectTree _tree;

        /// <summary>
        /// Object tree.
        /// </summary>
        [CLSCompliant(false)]
        public IObjectTree Tree
        {
            get
            {
                return _tree;
            }

            protected set
            {
                _tree = value;
            }
        }

        /// <summary>
        /// This event occurs when new documents are loaded.
        /// </summary>
        public event EventHandler<EventArgs> OnChanged;

        /// <summary>
        /// Indicates that if the specific OID is a table.
        /// </summary>
        /// <param name="id">OID</param>
        /// <returns></returns>
        internal bool IsTableId(uint[] id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            
            // TODO: enhance checking here.
            var name = Translate(id);
            return name.EndsWith("Table", StringComparison.Ordinal);
        }

        /// <summary>
        /// Validates if an <see cref="ObjectIdentifier"/> is a table.
        /// </summary>
        /// <param name="identifier">The object identifier.</param>
        /// <returns></returns>
        public bool ValidateTable(ObjectIdentifier identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }
                
            return IsTableId(identifier.ToNumerical());
        }

        /// <summary>
        /// Gets numercial form from textual form.
        /// </summary>
        /// <param name="textual">Textual</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public uint[] Translate(string textual)
        {
            if (textual == null)
            {
                throw new ArgumentNullException("textual");
            }
            
            if (textual.Length == 0)
            {
                throw new ArgumentException("textual cannot be empty");
            }
            
            var content = textual.Split(new[] { "::" }, StringSplitOptions.None);
            if (content.Length != 2)
            {
                throw new ArgumentException("textual format must be '<module>::<name>'");
            }
            
            return Translate(content[0], content[1]);
        }

        /// <summary>
        /// Gets numerical form from textual form.
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="name">Object name</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public uint[] Translate(string moduleName, string name)
        {
            if (moduleName == null)
            {
                throw new ArgumentNullException("moduleName");
            }
            
            if (moduleName.Length == 0)
            {
                throw new ArgumentException("module cannot be empty");
            }
            
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            
            if (name.Length == 0)
            {
                throw new ArgumentException("name cannot be empty");
            }
            
            if (!name.Contains("."))
            {
                return _tree.Find(moduleName, name).GetNumericalForm();
            }
            
            var content = name.Split('.');
            if (content.Length != 2)
            {
                throw new ArgumentException("name can only contain one dot");
            }
            
            int value;
            var succeeded = int.TryParse(content[1], out value);
            if (!succeeded)
            {
                throw new ArgumentException("not a decimal after dot");
            }
            
            var oid = _tree.Find(moduleName, content[0]).GetNumericalForm();
            return ObjectIdentifier.AppendTo(oid, (uint)value);
        }

        /// <summary>
        /// Gets textual form from numerical form.
        /// </summary>
        /// <param name="numerical">Numerical form</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public string Translate(uint[] numerical)
        {
            if (numerical == null)
            {
                throw new ArgumentNullException("numerical");
            }

            return Tree.Search(numerical).Text;
       }

        /// <summary>
        /// Loads a folder of MIB files.
        /// </summary>
        /// <param name="folder">Folder</param>
        /// <param name="pattern">MIB file pattern</param>
        public void CompileFolder(string folder, string pattern)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }
            
            if (folder.Length == 0)
            {
                throw new ArgumentException("folder cannot be empty");
            }
            
            var path = Path.GetFullPath(folder);
            
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("folder does not exist: " + path);
            }
            
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            
            if (pattern.Length == 0)
            {
                throw new ArgumentException("pattern cannot be empty");
            }
            
            CompileFiles(Directory.GetFiles(path, pattern));
        }

        /// <summary>
        /// Loads MIB files.
        /// </summary>
        /// <param name="fileNames">File names.</param>
        public void CompileFiles(IEnumerable<string> fileNames)
        {
            if (fileNames == null)
            {
                throw new ArgumentNullException("fileNames");
            }

            foreach (var fileName in fileNames)
            {
                Import(Parser.Compile(fileName));
            }
            
            Refresh();
        }

        /// <summary>
        /// Loads a MIB file.
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Compile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            
            if (fileName.Length == 0)
            {
                throw new ArgumentException("fileName cannot be empty");
            }
            
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("file does not exist: " + fileName);
            }
            
            Import(Parser.Compile(fileName));
            Refresh();
        }

        /// <summary>
        /// Imports instances of <see cref="MibModule"/>.
        /// </summary>
        /// <param name="modules">Modules.</param>
        public void Import(IEnumerable<IModule> modules)
        {
            _tree.Import(modules);
        }

        /// <summary>
        /// Refreshes.
        /// </summary>
        /// <remarks>This method raises an <see cref="OnChanged"/> event. </remarks>
        public void Refresh()
        {
            _tree.Refresh();
            var handler = OnChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates a variable.
        /// </summary>
        /// <param name="textual">The textual.</param>
        /// <returns></returns>
        public Variable CreateVariable(string textual)
        {
            return CreateVariable(textual, null);
        }

        /// <summary>
        /// Creates a variable.
        /// </summary>
        /// <param name="textual">The textual ID.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public Variable CreateVariable(string textual, ISnmpData data)
        {
            return new Variable(Translate(textual), data);
        }

        /// <summary>
        /// Decodes a variable using the loaded definitions to the best type.
        /// 
        /// Depending on the variable and loaded MIBs can return:
        ///     * Double
        ///     * Int32
        ///     * UInt32
        ///     * UInt64
        /// </summary>
        /// <param name="v">The variable to decode the value of.</param>
        /// <returns>The best result based on the loaded MIBs.</returns>
        public object Decode(Variable v)
        {
            return _tree.Decode(v);
        }
    }
}