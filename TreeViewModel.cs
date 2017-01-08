
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
	public class TreeViewModel: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
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
			viewState = CutViewState.Normal;
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
		
		private enum CutViewState {
			Normal = 0,
			Cut = 1,
			Paste = 2
		}
        TreeItem fromParent;
		string cutFrom;
		string pasteTo;
		private CutViewState viewState;
		
		internal void notifySel() {
			
			OnPropertyChanged("SelectedItem");
			
			if (!selectedItem.Selected) {
				return;
			}
			
			StateText = "Selected: " + selectedItem.Name;
			switch (viewState) {
				case TreeViewModel.CutViewState.Normal:
                    if (selectedItem.Name.Equals("Choices") )
                    {
                        return;
                    }
                    cutFrom = selectedItem.Name;
                    fromParent = selectedItem.parent;
					viewState = CutViewState.Cut;
					Operation = "Select a node to paste to";
					break;
				case TreeViewModel.CutViewState.Cut:
					if (selectedItem.Name.Equals(cutFrom)) {
						break;
					}
					viewState = CutViewState.Paste;
					pasteTo = selectedItem.Name;
                    if (fromParent != null )
                    {
                        var foundFrom = fromParent.TreeItems.First(t => t.Name.Equals(cutFrom));
                        if (foundFrom == null)
                        {
                            Operation = "Error! From " + cutFrom + " to " + pasteTo;
                        } else
                        {
                            fromParent.TreeItems.Remove(foundFrom);
                            selectedItem.TreeItems.Add(foundFrom);
                            foundFrom.parent = selectedItem;
                            Operation = "Done! From " + cutFrom + " to " + pasteTo;
                        }
                        
                    } else
                    {
                        Operation = "Error! From " + cutFrom + " to " + pasteTo;
                    }
                    
					break;
				case TreeViewModel.CutViewState.Paste:
					viewState = CutViewState.Normal;
					Operation = "Select a node to cut";
					HiddenStatus = "Choices";
					break;
				default:
					throw new Exception("Invalid value for CutViewState");
			}
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
