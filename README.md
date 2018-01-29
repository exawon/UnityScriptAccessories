# UnityScriptAccessories

## AnimatorBehaviour

`StateMachineBehaviour` is a component for state machine. Basically, one script per one state and each script has several functions. One single script for all state is possible but complicated. The more condition, the more painful it is to maintain.  

Coroutine is versatile feature to implement state machine. [AnimatorBehaviour](https://github.com/exawon/UnityScriptAccessories/blob/master/Miscellaneous/AnimatorBehaviour.cs) makes each coroutine synchronous to the state of Animator.  

`PlayControl` game object has `Animator` and `PlayControl` script.

![](https://raw.githubusercontent.com/exawon/UnityScriptAccessories/master/Images/animator-fsm-01.png)  

`Animator` controller has triggers for transition between states. `Exit Time` of a state is optional. `Motion` is not necessary.

![](https://raw.githubusercontent.com/exawon/UnityScriptAccessories/master/Images/animator-fsm-02.png)  

`AninamtorBehaviour` has a map of state to coroutine which has `AnimatorState` attribute. Every state event is included in one single coroutine. `PlayControl` script is like below:

```c#
public class PlayControl : AnimatorBehaviour
{
    //...
    
    [AnimatorState("Base Layer.Intro")]
    IEnumerator Intro(int layer, int nameHash)
    {
        //OnStateEnter
        
        while (StateCondition)
        {
            //OnStateUpdate
            yield return null;
        }
        
        //OnStateExit
    }

    [AnimatorState("Base Layer.PrePlaying")]
    IEnumerator Intro(int layer, int nameHash)
    {
        //...
    }

    //...
}
```

![](https://github.com/exawon/UnityScriptAccessories/blob/master/Images/bitmap-font-01.png)  

## ExcelImporter

[ExcelImporter](https://github.com/exawon/UnityScriptAccessories/blob/master/Editor/ExcelImporter.cs) import excel data directly using NPOI. There are two way to import.  
First way is importing as a `ScriptableObject`.  

![](https://github.com/exawon/UnityScriptAccessories/blob/master/Images/excel-importer-01.png)  

Second way is importing each data row into each prefab, in order to update many prefabs of same type by batch.  

![](https://github.com/exawon/UnityScriptAccessories/blob/master/Images/excel-importer-02.png)  

## BitmapFontImporter

Tools to generate a bitmap font:
- http://www.angelcode.com/products/bmfont/ (Windows)
- http://kvazars.com/littera/ (Web with Flash Player)

[BitmapFontImporter](https://github.com/exawon/UnityScriptAccessories/blob/master/Editor/BitmapFontImporter.cs) import a bitmap with XML format font data as a fontsettings asset embedded font material and texture.  

![](https://github.com/exawon/UnityScriptAccessories/blob/master/Images/bitmap-font-01.png)

## MessageSpellingChecker

[MessageSpellingChecker](https://github.com/exawon/UnityScriptAccessories/blob/master/Editor/MessageSpellingChecker.cs) check spelling of message of MonoBehaviour whenever the scripts are imported and reloaded.  

![](https://github.com/exawon/UnityScriptAccessories/blob/master/Images/misspelling-checker-01.png)

## Spline

[Spline](https://github.com/exawon/UnityScriptAccessories/tree/master/Spline) folder contains `SplineEditor`, `SplineData`, `SplineFollower`.

Linear curve is `B(t) = (1 - t) P0 + t P1`. The 2 dgree curve is `B(t) = (1 - t) ((1 - t) P0 + t P1) + t ((1 - t) P1 + t P2)`. It's `B(t) = (1 - t)^2 P0 + 2 (1 - t) t P1 + t^2 P2` compactly. And for the direction, the first derivative is `B'(t) = 2 (1 - t) (P1 - P0) + 2 t (P2 - P1)`.

![](https://github.com/exawon/UnityScriptAccessories/blob/master/Images/spline-editor-01.jpg)
