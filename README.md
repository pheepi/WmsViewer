# WMS Viewer

Viewer of geo-referenced maps with Web Map Service protocol (WMS) by Open Geospatial Consortium (OGC).

## Summary

Access to map services is possible through the web interface and also through the WMS service that allows its use for professionals working with varieties of GIS software, as well as other map servers where each map service can take and display them, together with their own local stored data.

WMS (Web Map Services) is a standardized protocol for communication with GIS servers. This service provides both information and geo-referenced maps with a simple interface. These services are available at predefined web addresses. WMS technology encrypts your requirement to the server's web address. In response, either raster maps or information in XML format describing the structure and properties provided by the maps are sent. Therefore, maps can be easily viewed in a web browser. OGC organization prepares the WMS standard.

Viewing the maps using a web browser is simple, but very inefficient. Each query to a web server contains a series of parameters to obtain exactly the image you want. Therefore, there are commercial and freeware programs that work like a simple browser. These programs maintain the configuration and automatically generate the web address of images displayed.

WMS servers can process two requests: "GetCapabilities" and "GetMap". The first request is used to obtain the XML file with the definition of a particular service. The basis of each service is a map tree that the user can download. Leaves of the tree represent individual and indivisible maps, nodes in the branches group their children. Each map has additional features such as title, description, image type, coordinate system, and much more. If we know all the relevant values, we can build the second request and ask for any image provided by the service.

## Documentation

The program is a simple Windows application. We will show you a small tutorial on how to load the service. First, you must connect to an existing service. In the address, there may be list of several maps of the world. You must press the "New WMS" button and enter the address, then the program will automatically download the information and create a new tree in TreeView in the lower right corner. This operation may take a while. After the successful creation of the new WMS, maps start to be downloading and viewing automatically.

The map view can be changed with the function buttons in the upper right corner. Click on the map, the map zooms closer or further and you can also drag it with mouse to move the map.

Each new WMS is shown in the TreeView as a tree structure of the individual layers. Layers can be visible, invisible (Checkbox), scroll up or down by pressing PageUp and PageDown. Maps can also be deleted with the Delete key. It depends on the order of the layers, the top maps will be displayed the first.

If you select a single layer, its configuration is shown in two side panels. The WMS service offered by this map is configured in the first panel. It is possible to read the name, description, version of the WMS protocol (1.1.1, for example) and the address where the service is located. It can be changed here, what coordinate system is used. The most common system is the EPSG: 4326 (or WGS 84), a GPS system, i.e. latitude and altitude. Downloading can be freezed, so the last downloaded map will be displayed.

The second panel represents the layer settings. Each layer has a name, description and a list of supported coordinate systems. You can select one of the supported image formats that will be downloaded and will be transparent according to the desired color. Each layer can be downloaded as a single image composed of all its sub-layers or each sub-layer separately (Download sublayers as one image). Image downloads faster, but if a sub-layer becomes invisible, a new image must be downloaded. This is a kind of optimization.

The program also has the option to undo. Use the Ctrl-Z and Ctrl-Y keys to return to the previous screen.

## License

This project is licensed under the terms of the BSD 3-clause "New" (or "Revised") license. See the [LICENSE.md](LICENSE.md) file for more details.
