
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
        internal abstract ICutViewState visitPaste2Normal(PasteState state);
        internal abstract ICutViewState visitNormal2Cut(NormalState state);
        internal abstract ICutViewState visitCut2Paste(CutState state);
    }
    class CutViewVisitor : ICutViewVisitor
    {
        

        override internal ICutViewState visitPaste2Normal(PasteState state)
        {
            var normalState = new NormalState();
            normalState.Operation = "Select a node to cut";
            normalState.HiddenStatus = "Choices";
            normalState.selectedItem = state.selectedItem;
            return normalState;
        }


        override internal ICutViewState visitNormal2Cut(NormalState state)
        {
            if (state.selectedItem.Name.Equals("Choices"))
            {
                return state;
            }
            var cutState = new CutState();
            cutState.cutFrom = state.selectedItem.Name;
            cutState.fromParent = state.selectedItem.parent;
            cutState.selectedItem = state.selectedItem;
            cutState.operation = state.operation;
            
            state.Operation = "Select a node to paste to";
            return cutState;
        }

        override internal ICutViewState visitCut2Paste(CutState state)
        {
            if (state.selectedItem.Name.Equals(state.cutFrom))
            {
                return state;
            }
            var pasteState = new PasteState();
            pasteState.selectedItem = state.selectedItem;
            pasteState.operation = state.operation;
            pasteState.pasteTo = state.selectedItem.Name;
            if (state.fromParent != null)
            {
                var foundFrom = state.fromParent.TreeItems.First(t => t.Name.Equals(state.cutFrom));
                if (foundFrom == null)
                {
                    pasteState.Operation = "Error! From " + state.cutFrom + " to " + pasteState.pasteTo;
                }
                else
                {
                    state.fromParent.TreeItems.Remove(foundFrom);
                    pasteState.selectedItem.TreeItems.Add(foundFrom);
                    foundFrom.parent = state.selectedItem;
                    pasteState.Operation = "Done! From " + state.cutFrom + " to " + pasteState.pasteTo;
                }

            }
            else
            {
                state.Operation = "Error! From " + state.cutFrom + " to " + pasteState.pasteTo;
            }
            return pasteState;
        }
        
    }


    public abstract class ICutViewState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        abstract internal ICutViewState accept(ICutViewVisitor visitor);
        internal string operation;
        public string Operation
        {
            get { return operation; }
            set
            {
                operation = value;
                OnPropertyChanged("operation");
            }
        }

        internal TreeItem selectedItem;
    }

    class NormalState : ICutViewState
    {
        //internal TreeViewModel model;
        override internal ICutViewState accept(ICutViewVisitor visitor)
        {
            return visitor.visitNormal2Cut(this);
        }

        private string hiddenStatus;
        public string HiddenStatus
        {
            get { return hiddenStatus; }
            set
            {
                hiddenStatus = value;
                OnPropertyChanged("HiddenStatus");
            }
        }
    }

    class CutState : ICutViewState
    {
        //internal TreeViewModel model;
        override internal ICutViewState accept(ICutViewVisitor visitor)
        {
            return visitor.visitCut2Paste(this);
        }
        internal string cutFrom;
        internal TreeItem fromParent;
    }
    class PasteState : ICutViewState
    {
        internal string pasteTo;
        //internal TreeViewModel model;
        override internal ICutViewState accept(ICutViewVisitor visitor)
        {
            return visitor.visitPaste2Normal(this);
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

			viewState.operation = "Select a node to cut";
		}
		
		private string stateText;
		public string StateText {
			get { return stateText; }
			set { 
				stateText = value; 
				OnPropertyChanged("StateText");
			}
		}
				
		internal ICutViewState viewState = new NormalState();
        public ICutViewState ViewState
        {
            get { return viewState; }
            set
            {
                viewState = value;
                OnPropertyChanged("ViewState");
            }
        }

        internal void notifySel() {
			
			OnPropertyChanged("SelectedItem");
			
			if (!viewState.selectedItem.Selected) {
				return;
			}
			
			StateText = "Selected: " + viewState.selectedItem.Name;

            ViewState = viewState.accept(cutVisitor);
		}
		
		
		public TreeItem SelectedItem {
			get { return viewState.selectedItem; }
			set {
				treeItems[0].Selected = false;
				foreach (TreeItem item in treeItems[0].TreeItems.Where(i => i.Selected)) {
					item.Selected = false;	
				}
                viewState.selectedItem = value;
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
					model.viewState.selectedItem = this;
					model.notifySel();
				}
			}
			
		}
		
	}
}
