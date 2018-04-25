[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.MapBox/master/Shared/Icon.png "Zebble.MapBox"


## Zebble.MapBox

![logo]

A Zebble plugin to make you able to have extra options to use a map.


[![NuGet](https://img.shields.io/nuget/v/Zebble.MapBox.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.MapBox/)

> If the functionality provided by the native map components is not enough, then you can use the MapBox Plugin for Zebble. It has some advanced features such as custom colour themes, data visualizations and navigation.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.MapBox/](https://www.nuget.org/packages/Zebble.MapBox/)
* Install in your platform client projects.
* Available for iOS, Android.
<br>


### Api Usage

You can use below codes for showing MapBox:
```csharp
<Plugin.MapBox  Center="51.5074, 0.1278" Zoom="13" />
```
For more information please refer to https://www.mapbox.com
<br>

### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| AccessToken           | string          | x       | x   | x       |
| StyleUrl           | string          | x       | x   | x       | 
| AnnotationImagePath           | string          | x       | x   | x       |
| AnnotationImageSize           | Size          | x       | x   | x      |
| ShowsUserLocation           | bool          | x       | x   | x       |
| Zoom           | float          | x       | x   | x       |
| Annotations           | IEnumerable<Annotation&gt;          | x       | x   | x       |
| Center        | Services.GeoLocation | x | x | x |

<br>

### Events
| Event             | Type                                          | Android | iOS | Windows |
| :-----------      | :-----------                                  | :------ | :-- | :------ |
| AnnotationClicked            | Action<Annotation&gt;    | x       | x   |        |


<br>


### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| Add         | void         | annotation -> Annotation[] | x       | x   | x        |
| FitBounds         | void         | start -> GeoLocation, end -> GeoLocation | x       | x   | x      |
| Remove | void | annotation -> Annotation[]| x | x | x
