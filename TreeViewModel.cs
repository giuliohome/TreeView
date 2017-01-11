
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


    abstract class ICutViewVisitor<A>
    {
        internal abstract A visitPaste2Normal(PasteState pasteState);
        internal abstract A visitNormal2Cut(NormalState normalState);
        internal abstract A visitCut2Paste(CutState cutState);
    }
    class CutViewVisitor : ICutViewVisitor<ICutViewState>
    {
        

        override internal ICutViewState visitPaste2Normal(PasteState pasteState)
        {
            var normalState = new NormalState();
            normalState.Operation = "Select a node to cut";
            normalState.HiddenStatus = "Choices";
            normalState.selectedItem = pasteState.selectedItem;
            normalState.StateText = "Selected: " + normalState.selectedItem.Name;
            return normalState;
        }


        override internal ICutViewState visitNormal2Cut(NormalState normalState)
        {
            if (normalState.selectedItem.Name.Equals("Choices"))
            {
                normalState.StateText = "Selected: " + normalState.selectedItem.Name;
                return normalState;
            }
            var cutState = new CutState();
            cutState.cutFrom = normalState.selectedItem.Name;
            cutState.fromParent = normalState.selectedItem.parent;
            cutState.selectedItem = normalState.selectedItem;
            cutState.operation = normalState.operation;

            cutState.Operation = "Select a node to paste to";
            cutState.StateText = "Selected: " + cutState.selectedItem.Name;
            return cutState;
        }

        override internal ICutViewState visitCut2Paste(CutState cutState)
        {
            if (cutState.selectedItem.Name.Equals(cutState.cutFrom))
            {
                cutState.StateText = "Selected: " + cutState.selectedItem.Name;
                return cutState;
            }
            var pasteState = new PasteState();
            pasteState.selectedItem = cutState.selectedItem;
            pasteState.operation = cutState.operation;
            pasteState.pasteTo = cutState.selectedItem.Name;
            if (cutState.fromParent != null)
            {
                var foundFrom = cutState.fromParent.TreeItems.First(t => t.Name.Equals(cutState.cutFrom));
                if (foundFrom == null)
                {
                    cutState.StateText = "Error! From " + cutState.cutFrom + " to " + pasteState.pasteTo;
                    return cutState;
                }
                else
                {
                    var checkParent = pasteState.selectedItem.parent;
                    bool checkCyclic = false;
                    while (checkParent != null)
                    {
                        if (foundFrom.Name.Equals(checkParent.Name))
                        {
                            checkCyclic = true;
                            break;
                        }
                        checkParent = checkParent.parent;
                    }
                    if (checkCyclic)
                    {
                        cutState.StateText = "Cyclic Reference! From " + cutState.cutFrom + " to " + pasteState.pasteTo;
                        return cutState;
                    }
                    cutState.fromParent.TreeItems.Remove(foundFrom);
                    pasteState.selectedItem.TreeItems.Add(foundFrom);
                    foundFrom.parent = cutState.selectedItem;
                    pasteState.Operation = "Done! From " + cutState.cutFrom + " to " + pasteState.pasteTo;
                }

            }
            else
            {
                cutState.StateText = "Error! From " + cutState.cutFrom + " to " + pasteState.pasteTo;
                return cutState;
            }
            pasteState.StateText = "Selected: " + pasteState.selectedItem.Name;
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
        abstract internal A accept<A> (ICutViewVisitor<A> visitor);
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

        private string stateText;
        public string StateText
        {
            get { return stateText; }
            set
            {
                stateText = value;
                OnPropertyChanged("StateText");
            }
        }

        internal TreeItem selectedItem;
    }

    class NormalState : ICutViewState
    {
        //internal TreeViewModel model;
        override internal A accept<A>(ICutViewVisitor<A> visitor)
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
        override internal A accept<A>(ICutViewVisitor<A> visitor)
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
        override internal A accept<A>(ICutViewVisitor<A> visitor)
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
