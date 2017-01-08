# TreeView
State handling in WPF-MVVM


I  suggest a functional approach and I hope that a simple example related to the button' s state (enabled or disabled) can be generic enough ...

So that's how I usually manage this: I simply go in the CanRun method of the DelegateCommand, implementing the button's ICommand. It must have a getter of course.

There I write - in a very functional approach - the rule underlying its state, like for instance if a certain field1 (behind property1) has a value and a field2 has another value or field3 is something else.

Finally I just go into all the setters of the involved properties (let's say 1, 2 and 3) and there I raise the CanExecuteChanged of the button's command, immediately after the OnPropertyChanged of the property.

But I would do the same thing for any dependent property: no setter in the dependent, only the getter with a functional rule and then I only have to add the dependent OnPropertyChanged for each dependency properties involved in its state's rule definition, after their own OnPropertyChanged.

Imho It's a very easy way to define the state's rules inside the view model, for either a command or a property.

As far as the XAML is concerned, if there is a more complex effect to be triggered than just the standard behaviors of a button and a text box (or the likes), I guess that the most elegant solution is to write an attached behavior... even though an invisible element with a code behind could quickly replace the definition of an attached behavior (ultimately "behaviors package up code" that you would normally have to write as code behind, because it directly interacts with the API of XAML elements). 

[Here](https://giuliohome.wordpress.com/2017/01/08/state-handling-in-wpf-mvvm/) is the source code of a TreeView demo example with 3 states handled in the ViewModel.

This repository contains a more complete demo, with multiple levels and a working copy&paste functionality.
