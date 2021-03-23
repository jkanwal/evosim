# Data Visualisation Implementation Guide

## Added Code
1. Assets -> Animation
    - Graph Menu Animation Controller
    - Graph Menu Opening Animation
2. Assets -> Chart and Graph
    - Package Elements
3. Assets -> Scripts
    - GraphMenu.cs : Controls Graph Menu behaviour.
    - RunSim.cs : Added functions to aid graph control and update data points.
    - CreatureBehaviour.cs : Added singular lines to update some graphing variables in RunSim.cs.
4. Assets -> Scenes
    - Scene0.unity : Example implementation of the graph menu functionality


## Adding New Graphs
(Using Scene0.unity)
1. Within the Hierarchy, expand Canvas->ScrollView->Viewport->Content.
2. Right click Content and add a new UI->Panel element.
3. In the newly created panel, graphs can be added by going to Tools->Charts->Desired Graph in the toolbar.
4. For Graph Charts, a Category needs to be added which will be updated when data points are added (this can be done in code as well if desired).
5. To allow the code to access the new graph, it can be added as an attribute within RunSim.cs (class is GraphChart, PieChart, BarChart etc.).
6. To add a category to the graph, the method AddCategory can be run on the charts DataSource.
7. Adding a data point to a graph can be done through either the AddPointToCategoryRealTime or the AddPointToCategory methods on the charts DataSource  (or the method SetValue for pie/bar charts). Realtime updates are normally used for the current generation polls to allow for rolling functionality.
8. The horizontal scale of a graph can be altered by changing the HorizontalViewSize and HorizontalViewOrigin attributes of the DataSource (with the same method for vertical axis). The function alterScale in RunSim.cs can also be called to dynamically update scales as data points fluctuate.
9. Finally, to allow for the new graph to be toggled on/off, select the first Panel in the Content object described in step 1.
10. In this Panel, add a new UI->Toggle element.
11. With this new Toggle element selected, alter the OnValueChanged functionality to call the RunSim.toggleGraph function, and pass the Panel that you put your new graph in as the argument.
12. For completeness, expand the Toggle element and select the Label element to change the text tag for the toggle so that the user knows what the toggle box does.
