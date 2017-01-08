
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TreeVisitor
{
    /// <summary>
    /// Description of TreeViewModelcs.
    /// An example of functional state mgmt 
    /// </summary>
    /// 


    abstract class ICutViewVisitor
    {
        internal abstract void visit(PasteState state);
        internal abstract void visit(NormalState state);
        internal abstract void visit(CutState state);
    }
    class CutViewVisitor : ICutViewVisitor
    {
        

        override internal void visit(PasteState state)
        {
            state.model.viewState = state.model.normalState;
            state.model.Operation = "Select a node to cut";
            state.model.HiddenStatus = "Choices";
        }


        override internal void visit(NormalState state)
        {
            if (state.model.selectedItem.Name.Equals("Choices"))
            {
                return;
            }
            state.model.cutFrom = state.model.selectedItem.Name;
            state.model.fromParent = state.model.selectedItem.parent;
            state.model.viewState = state.model.cutState;
            state.model.Operation = "Select a node to paste to";
        }

        override internal void visit(CutState state)
        {
            if (state.model.selectedItem.Name.Equals(state.model.cutFrom))
            {
                return;
            }
            state.model.viewState = state.model.pasteState;
            state.model.pasteTo = state.model.selectedItem.Name;
            if (state.model.fromParent != null)
            {
                var foundFrom = state.model.fromParent.TreeItems.First(t => t.Name.Equals(state.model.cutFrom));
                if (foundFrom == null)
                {
                    state.model.Operation = "Error! From " + state.model.cutFrom + " to " + state.model.pasteTo;
                }
                else
                {
                    state.model.fromParent.TreeItems.Remove(foundFrom);
                    state.model.selectedItem.TreeItems.Add(foundFrom);
                    foundFrom.parent = state.model.selectedItem;
                    state.model.Operation = "Done! From " + state.model.cutFrom + " to " + state.model.pasteTo;
                }

            }
            else
            {
                state.model.Operation = "Error! From " + state.model.cutFrom + " to " + state.model.pasteTo;
            }
        }
        
    }


    abstract class ICutViewState
    {
        abstract internal void accept(ICutViewVisitor visitor);
    }

    class NormalState : ICutViewState
    {
        internal TreeViewModel model;
        override internal void accept(ICutViewVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    class CutState : ICutViewState
    {
        internal TreeViewModel model;
        override internal void accept(ICutViewVisitor visitor)
        {
            visitor.visit(this);
        }
    }
    class PasteState : ICutViewState
    {
        internal TreeViewModel model;
        override internal void accept(ICutViewVisitor visitor)
        {
            visitor.visit(this);
        }
    }



    public class TreeViewModel: INotifyPropertyChanged
    {
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

        internal NormalState normalState = new NormalState();
        internal CutState cutState = new CutState();
        internal PasteState pasteState = new PasteState();
        internal CutViewVisitor cutVisitor = new CutViewVisitor();

		public TreeViewModel()
		{
            var root = new TreeItem()
            {
                Name = "Choices",
                model = this,
                Selected = false
            };
            var xTreeItems = new ObservableCollection<TreeItem>(
			Enumerable.Range(1,10)
				.Select( n => {
				        	return new TreeItem() {
			        			model = this, parent = root,
				        		Name = "N. " + n,
				        		Selected = false
				        	};
			        }));
            root.TreeItems = xTreeItems;
            treeItems = new ObservableCollection<TreeItem>();
			treeItems.Add(root);
            normalState.model = this;
            cutState.model = this;
            pasteState.model = this;
			viewState = normalState;
			operation = "Select a node to cut";
		}
		
		private string stateText;
		public string StateText {
			get { return stateText; }
			set { 
				stateText = value; 
				OnPropertyChanged("StateText");
			}
		}
		
		private string operation;
		public string Operation {
			get { return operation; }
			set { 
				operation = value; 
				OnPropertyChanged("operation");
			}
		}
		
		private string hiddenStatus;
		public string HiddenStatus {
			get { return hiddenStatus; }
			set { 
				hiddenStatus = value; 
				OnPropertyChanged("HiddenStatus");
			}
		}

        
        internal TreeItem fromParent;
		internal string cutFrom;
		internal string pasteTo;
		internal ICutViewState viewState;
		
		internal void notifySel() {
			
			OnPropertyChanged("SelectedItem");
			
			if (!selectedItem.Selected) {
				return;
			}
			
			StateText = "Selected: " + selectedItem.Name;

            viewState.accept(cutVisitor);
		}
		
		internal TreeItem selectedItem;
		public TreeItem SelectedItem {
			get { return selectedItem; }
			set {
				treeItems[0].Selected = false;
				foreach (TreeItem item in treeItems[0].TreeItems.Where(i => i.Selected)) {
					item.Selected = false;	
				}
				selectedItem = value;
				OnPropertyChanged("SelectedItem");
			}
		}
		
		ObservableCollection<TreeItem> treeItems = new ObservableCollection<TreeItem>();
		public  ObservableCollection<TreeItem> TreeItems {
			get { return treeItems; }
			set {
				treeItems = value;
			}
		}
			
		
	}
	
	public class TreeItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		ObservableCollection<TreeItem> treeItems = new ObservableCollection<TreeItem>();
		public  ObservableCollection<TreeItem> TreeItems {
			get { return treeItems; }
			set {
				treeItems = value;
			}
		}
		
		private string name;
		public string Name {
			get { return name; }
			set {
				name = value;
				OnPropertyChanged("Name");
			}
			
		}
        internal TreeItem parent;
		internal TreeViewModel model;
		private bool selected;
		public bool Selected {
			get { return selected; }
			set {
				selected = value;
				OnPropertyChanged("Selected");
				if (model != null) {
					model.selectedItem = this;
					model.notifySel();
				}
			}
			
		}
		
	}
}
