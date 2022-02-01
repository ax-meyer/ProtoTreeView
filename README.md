# ProtoTreeView

C# WPF library offering a basic nested tree view for protobuf files.

![Screenshot of tree view](viewer_example.png)

## Usage
- Use nuget package: https://www.nuget.org/packages/ProtoTreeView/
- OR: Integrate the ProtoTreeView project in your project
- Add `using ProtoTreeView`
- Call `ProtoTreeViewWindow treeViewWindow = new ProtoTreeViewWindow(proto message); treeViewWindow.Show();`

or 

- Use the `ProtoTreeViewUserControl` to integrate the tree view into your existing UI with a MVVM pattern.
