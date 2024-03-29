﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ICSharpCode.TreeView
{
    public partial class SharpTreeNode : INotifyPropertyChanged
    {
        private SharpTreeNodeCollection modelChildren;
        internal SharpTreeNode modelParent;
        private bool isVisible = true;

        private void UpdateIsVisible(bool parentIsVisible, bool updateFlattener)
        {
            bool newIsVisible = parentIsVisible && !isHidden;
            if (isVisible != newIsVisible)
            {
                isVisible = newIsVisible;

                // invalidate the augmented data
                SharpTreeNode node = this;
                while (node != null && node.totalListLength >= 0)
                {
                    node.totalListLength = -1;
                    node = node.listParent;
                }
                // Remember the removed nodes:
                List<SharpTreeNode> removedNodes = null;
                if (updateFlattener && !newIsVisible)
                {
                    removedNodes = VisibleDescendantsAndSelf().ToList();
                }
                // also update the model children:
                UpdateChildIsVisible(false);

                // Validate our invariants:
                if (updateFlattener)
                {
                    CheckRootInvariants();
                }

                // Tell the flattener about the removed nodes:
                if (removedNodes != null)
                {
                    TreeFlattener flattener = GetListRoot().treeFlattener;
                    if (flattener != null)
                    {
                        flattener.NodesRemoved(GetVisibleIndexForNode(this), removedNodes);
                        foreach (SharpTreeNode n in removedNodes)
                        {
                            n.OnIsVisibleChanged();
                        }
                    }
                }
                // Tell the flattener about the new nodes:
                if (updateFlattener && newIsVisible)
                {
                    TreeFlattener flattener = GetListRoot().treeFlattener;
                    if (flattener != null)
                    {
                        flattener.NodesInserted(GetVisibleIndexForNode(this), VisibleDescendantsAndSelf());
                        foreach (SharpTreeNode n in VisibleDescendantsAndSelf())
                        {
                            n.OnIsVisibleChanged();
                        }
                    }
                }
            }
        }

        protected virtual void OnIsVisibleChanged() { }

        private void UpdateChildIsVisible(bool updateFlattener)
        {
            if (modelChildren != null && modelChildren.Count > 0)
            {
                bool showChildren = isVisible && isExpanded;
                foreach (SharpTreeNode child in modelChildren)
                {
                    child.UpdateIsVisible(showChildren, updateFlattener);
                }
            }
        }

        #region Main

        public SharpTreeNode()
        {
        }

        public SharpTreeNodeCollection Children
        {
            get
            {
                if (modelChildren == null)
                {
                    modelChildren = new SharpTreeNodeCollection(this);
                }

                return modelChildren;
            }
        }

        public SharpTreeNode Parent => modelParent;

        public virtual object Text => null;

        public virtual Brush Foreground => SystemColors.WindowTextBrush;

        public virtual object Icon => null;

        public virtual object ToolTip => null;

        public int Level => Parent != null ? Parent.Level + 1 : 0;

        public bool IsRoot => Parent == null;

        private bool isHidden;

        public bool IsHidden
        {
            get => isHidden;
            set
            {
                if (isHidden != value)
                {
                    isHidden = value;
                    if (modelParent != null)
                    {
                        UpdateIsVisible(modelParent.isVisible && modelParent.isExpanded, true);
                    }

                    RaisePropertyChanged("IsHidden");
                    if (Parent != null)
                    {
                        Parent.RaisePropertyChanged("ShowExpander");
                    }
                }
            }
        }

        /// <summary>
        /// Return true when this node is not hidden and when all parent nodes are expanded and not hidden.
        /// </summary>
        public bool IsVisible => isVisible;

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region OnChildrenChanged
        protected internal virtual void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SharpTreeNode node in e.OldItems)
                {
                    Debug.Assert(node.modelParent == this);
                    node.modelParent = null;
                    Debug.WriteLine("Removing {0} from {1}", node, this);
                    SharpTreeNode removeEnd = node;
                    while (removeEnd.modelChildren != null && removeEnd.modelChildren.Count > 0)
                    {
                        removeEnd = removeEnd.modelChildren.Last();
                    }

                    List<SharpTreeNode> removedNodes = null;
                    int visibleIndexOfRemoval = 0;
                    if (node.isVisible)
                    {
                        visibleIndexOfRemoval = GetVisibleIndexForNode(node);
                        removedNodes = node.VisibleDescendantsAndSelf().ToList();
                    }

                    RemoveNodes(node, removeEnd);

                    if (removedNodes != null)
                    {
                        TreeFlattener flattener = GetListRoot().treeFlattener;
                        if (flattener != null)
                        {
                            flattener.NodesRemoved(visibleIndexOfRemoval, removedNodes);
                        }
                    }
                }
            }
            if (e.NewItems != null)
            {
                SharpTreeNode insertionPos;
                if (e.NewStartingIndex == 0)
                {
                    insertionPos = null;
                }
                else
                {
                    insertionPos = modelChildren[e.NewStartingIndex - 1];
                }

                foreach (SharpTreeNode node in e.NewItems)
                {
                    Debug.Assert(node.modelParent == null);
                    node.modelParent = this;
                    node.UpdateIsVisible(isVisible && isExpanded, false);
                    //Debug.WriteLine("Inserting {0} after {1}", node, insertionPos);

                    while (insertionPos != null && insertionPos.modelChildren != null && insertionPos.modelChildren.Count > 0)
                    {
                        insertionPos = insertionPos.modelChildren.Last();
                    }
                    InsertNodeAfter(insertionPos ?? this, node);

                    insertionPos = node;
                    if (node.isVisible)
                    {
                        TreeFlattener flattener = GetListRoot().treeFlattener;
                        if (flattener != null)
                        {
                            flattener.NodesInserted(GetVisibleIndexForNode(node), node.VisibleDescendantsAndSelf());
                        }
                    }
                }
            }

            RaisePropertyChanged("ShowExpander");
            RaiseIsLastChangedIfNeeded(e);
        }
        #endregion

        #region Expanding / LazyLoading

        public virtual object ExpandedIcon => Icon;

        public virtual bool ShowExpander => LazyLoading || Children.Any(c => !c.isHidden);

        private bool isExpanded;

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    if (isExpanded)
                    {
                        EnsureLazyChildren();
                        OnExpanding();
                    }
                    else
                    {
                        OnCollapsing();
                    }
                    UpdateChildIsVisible(true);
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }

        protected virtual void OnExpanding() { }
        protected virtual void OnCollapsing() { }

        private bool lazyLoading;

        public bool LazyLoading
        {
            get => lazyLoading;
            set
            {
                lazyLoading = value;
                if (lazyLoading)
                {
                    IsExpanded = false;
                    if (canExpandRecursively)
                    {
                        canExpandRecursively = false;
                        RaisePropertyChanged("CanExpandRecursively");
                    }
                }
                RaisePropertyChanged("LazyLoading");
                RaisePropertyChanged("ShowExpander");
            }
        }

        private bool canExpandRecursively = true;

        /// <summary>
        /// Gets whether this node can be expanded recursively.
        /// If not overridden, this property returns false if the node is using lazy-loading, and true otherwise.
        /// </summary>
        public virtual bool CanExpandRecursively => canExpandRecursively;

        public virtual bool ShowIcon => Icon != null;

        protected virtual void LoadChildren()
        {
            throw new NotSupportedException(GetType().Name + " does not support lazy loading");
        }

        /// <summary>
        /// Ensures the children were initialized (loads children if lazy loading is enabled)
        /// </summary>
        public void EnsureLazyChildren()
        {
            if (LazyLoading)
            {
                LazyLoading = false;
                LoadChildren();
            }
        }

        #endregion

        #region Ancestors / Descendants

        public IEnumerable<SharpTreeNode> Descendants()
        {
            return TreeTraversal.PreOrder(Children, n => n.Children);
        }

        public IEnumerable<SharpTreeNode> DescendantsAndSelf()
        {
            return TreeTraversal.PreOrder(this, n => n.Children);
        }

        internal IEnumerable<SharpTreeNode> VisibleDescendants()
        {
            return TreeTraversal.PreOrder(Children.Where(c => c.isVisible), n => n.Children.Where(c => c.isVisible));
        }

        internal IEnumerable<SharpTreeNode> VisibleDescendantsAndSelf()
        {
            return TreeTraversal.PreOrder(this, n => n.Children.Where(c => c.isVisible));
        }

        public IEnumerable<SharpTreeNode> Ancestors()
        {
            for (SharpTreeNode n = Parent; n != null; n = n.Parent)
            {
                yield return n;
            }
        }

        public IEnumerable<SharpTreeNode> AncestorsAndSelf()
        {
            for (SharpTreeNode n = this; n != null; n = n.Parent)
            {
                yield return n;
            }
        }

        #endregion

        #region Editing

        public virtual bool IsEditable => false;

        private bool isEditing;

        public bool IsEditing
        {
            get => isEditing;
            set
            {
                if (isEditing != value)
                {
                    isEditing = value;
                    RaisePropertyChanged("IsEditing");
                }
            }
        }

        public virtual string LoadEditText()
        {
            return null;
        }

        public virtual bool SaveEditText(string value)
        {
            return true;
        }

        #endregion

        #region Checkboxes

        public virtual bool IsCheckable => false;

        private bool? isChecked;

        public bool? IsChecked
        {
            get => isChecked;
            set => SetIsChecked(value, true);
        }

        private void SetIsChecked(bool? value, bool update)
        {
            if (isChecked != value)
            {
                isChecked = value;

                if (update)
                {
                    if (IsChecked != null)
                    {
                        foreach (SharpTreeNode child in Descendants())
                        {
                            if (child.IsCheckable)
                            {
                                child.SetIsChecked(IsChecked, false);
                            }
                        }
                    }

                    foreach (SharpTreeNode parent in Ancestors())
                    {
                        if (parent.IsCheckable)
                        {
                            if (!parent.TryValueForIsChecked(true))
                            {
                                if (!parent.TryValueForIsChecked(false))
                                {
                                    parent.SetIsChecked(null, false);
                                }
                            }
                        }
                    }
                }

                RaisePropertyChanged("IsChecked");
            }
        }

        private bool TryValueForIsChecked(bool? value)
        {
            if (Children.Where(n => n.IsCheckable).All(n => n.IsChecked == value))
            {
                SetIsChecked(value, false);
                return true;
            }
            return false;
        }

        #endregion

        #region Cut / Copy / Paste / Delete

        /// <summary>
        /// Gets whether the node should render transparently because it is 'cut' (but not actually removed yet).
        /// </summary>
        public virtual bool IsCut => false;
        /*
			static List<SharpTreeNode> cuttedNodes = new List<SharpTreeNode>();
			static IDataObject cuttedData;
			static EventHandler requerySuggestedHandler; // for weak event
	
			static void StartCuttedDataWatcher()
			{
				requerySuggestedHandler = new EventHandler(CommandManager_RequerySuggested);
				CommandManager.RequerySuggested += requerySuggestedHandler;
			}
	
			static void CommandManager_RequerySuggested(object sender, EventArgs e)
			{
				if (cuttedData != null && !Clipboard.IsCurrent(cuttedData)) {
					ClearCuttedData();
				}
			}
	
			static void ClearCuttedData()
			{
				foreach (var node in cuttedNodes) {
					node.IsCut = false;
				}
				cuttedNodes.Clear();
				cuttedData = null;
			}
	
			//static public IEnumerable<SharpTreeNode> PurifyNodes(IEnumerable<SharpTreeNode> nodes)
			//{
			//    var list = nodes.ToList();
			//    var array = list.ToArray();
			//    foreach (var node1 in array) {
			//        foreach (var node2 in array) {
			//            if (node1.Descendants().Contains(node2)) {
			//                list.Remove(node2);
			//            }
			//        }
			//    }
			//    return list;
			//}
	
			bool isCut;
	
			public bool IsCut
			{
				get { return isCut; }
				private set
				{
					isCut = value;
					RaisePropertyChanged("IsCut");
				}
			}
	
			internal bool InternalCanCut()
			{
				return InternalCanCopy() && InternalCanDelete();
			}
	
			internal void InternalCut()
			{
				ClearCuttedData();
				cuttedData = Copy(ActiveNodesArray);
				Clipboard.SetDataObject(cuttedData);
	
				foreach (var node in ActiveNodes) {
					node.IsCut = true;
					cuttedNodes.Add(node);
				}
			}
	
			internal bool InternalCanCopy()
			{
				return CanCopy(ActiveNodesArray);
			}
	
			internal void InternalCopy()
			{
				Clipboard.SetDataObject(Copy(ActiveNodesArray));
			}
	
			internal bool InternalCanPaste()
			{
				return CanPaste(Clipboard.GetDataObject());
			}
	
			internal void InternalPaste()
			{
				Paste(Clipboard.GetDataObject());
	
				if (cuttedData != null) {
					DeleteCore(cuttedNodes.ToArray());
					ClearCuttedData();
				}
			}
		 */

        public virtual bool CanDelete(SharpTreeNode[] nodes)
        {
            return false;
        }

        public virtual void Delete(SharpTreeNode[] nodes)
        {
            throw new NotSupportedException(GetType().Name + " does not support deletion");
        }

        public virtual void DeleteWithoutConfirmation(SharpTreeNode[] nodes)
        {
            throw new NotSupportedException(GetType().Name + " does not support deletion");
        }

        public virtual bool CanCut(SharpTreeNode[] nodes)
        {
            return CanCopy(nodes) && CanDelete(nodes);
        }

        public virtual void Cut(SharpTreeNode[] nodes)
        {
            IDataObject data = GetDataObject(nodes);
            if (data != null)
            {
                // TODO: default cut implementation should not immediately perform deletion, but use 'IsCut'
                Clipboard.SetDataObject(data, copy: true);
                DeleteWithoutConfirmation(nodes);
            }
        }

        public virtual bool CanCopy(SharpTreeNode[] nodes)
        {
            return false;
        }

        public virtual void Copy(SharpTreeNode[] nodes)
        {
            IDataObject data = GetDataObject(nodes);
            if (data != null)
            {
                Clipboard.SetDataObject(data, copy: true);
            }
        }

        protected virtual IDataObject GetDataObject(SharpTreeNode[] nodes)
        {
            return null;
        }

        public virtual bool CanPaste(IDataObject data)
        {
            return false;
        }

        public virtual void Paste(IDataObject data)
        {
            throw new NotSupportedException(GetType().Name + " does not support copy/paste");
        }
        #endregion

        #region Drag and Drop
        public virtual void StartDrag(DependencyObject dragSource, SharpTreeNode[] nodes)
        {
            // The default drag implementation works by reusing the copy infrastructure.
            // Derived classes should override this method
            IDataObject data = GetDataObject(nodes);
            if (data == null)
            {
                return;
            }

            DragDropEffects effects = DragDropEffects.Copy;
            if (CanDelete(nodes))
            {
                effects |= DragDropEffects.Move;
            }

            DragDropEffects result = DragDrop.DoDragDrop(dragSource, data, effects);
            if (result == DragDropEffects.Move)
            {
                DeleteWithoutConfirmation(nodes);
            }
        }

        /// <summary>
        /// Gets the possible drop effects.
        /// If the method returns more than one of (Copy|Move|Link), the tree view will choose one effect based
        /// on the allowed effects and keyboard status.
        /// </summary>
        public virtual DragDropEffects GetDropEffect(DragEventArgs e, int index)
        {
            // Since the default drag implementation uses Copy(),
            // we'll use Paste() in our default drop implementation.
            if (CanPaste(e.Data))
            {
                // If Ctrl is pressed -> copy
                // If moving is not allowed -> copy
                // Otherwise: move
                if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0 || (e.AllowedEffects & DragDropEffects.Move) == 0)
                {
                    return DragDropEffects.Copy;
                }

                return DragDropEffects.Move;
            }
            return DragDropEffects.None;
        }

        internal void InternalDrop(DragEventArgs e, int index)
        {
            if (LazyLoading)
            {
                EnsureLazyChildren();
                index = Children.Count;
            }

            Drop(e, index);
        }

        public virtual void Drop(DragEventArgs e, int index)
        {
            // Since the default drag implementation uses Copy(),
            // we'll use Paste() in our default drop implementation.
            Paste(e.Data);
        }
        #endregion

        #region IsLast (for TreeView lines)

        public bool IsLast => Parent == null ||
                    Parent.Children[Parent.Children.Count - 1] == this;

        private void RaiseIsLastChangedIfNeeded(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex == Children.Count - 1)
                    {
                        if (Children.Count > 1)
                        {
                            Children[Children.Count - 2].RaisePropertyChanged("IsLast");
                        }
                        Children[Children.Count - 1].RaisePropertyChanged("IsLast");
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex == Children.Count)
                    {
                        if (Children.Count > 0)
                        {
                            Children[Children.Count - 1].RaisePropertyChanged("IsLast");
                        }
                    }
                    break;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Model
        /// <summary>
        /// Gets the underlying model object.
        /// </summary>
        /// <remarks>
        /// This property calls the virtual <see cref="GetModel()"/> helper method.
        /// I didn't make the property itself virtual because deriving classes
        /// may wish to replace it with a more specific return type,
        /// but C# doesn't support variance in override declarations.
        /// </remarks>
        public object Model => GetModel();

        protected virtual object GetModel()
        {
            return null;
        }
        #endregion

        /// <summary>
        /// Gets called when the item is double-clicked.
        /// </summary>
        public virtual void ActivateItem(RoutedEventArgs e)
        {
        }

        public virtual void ShowContextMenu(ContextMenuEventArgs e)
        {
        }

        public override string ToString()
        {
            // used for keyboard navigation
            object text = Text;
            return text != null ? text.ToString() : string.Empty;
        }
    }
}
