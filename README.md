# AspNetMvcUrlHelper
using symbol not string to build url route.

#How
```cs
public class SomeController : Controller
{
    public ActionResult Index(){
        return Content("Index");
    }
    
    public ActionResult Search(string word){
      return Content("Search");
    }
}

// /Some/Index
var url = Url.Action<SomeController>(p=>p.Index());

// /Some/Search?word=sean
var word = "sean";
var url = Url.Action<SomeController>(p=>p.Search(word));
```
#HtmlHelper
```cs
//before
Html.Action("Search","Some",new {word="sean"});
//after
Html.Action<SomeController>(p=>p.Search("sean"));

//before
Html.RenderAction("Search","Some",new {word="sean"});
//after
Html.RenderAction<SomeController>(p=>p.Search("sean"));
```

#UrlHelper
```cs
//before
Url.Action("Search","Some",new {word="sean"});
//after
Url.Action<SomeController>(p=>p.Search("sean"));
```

#Code in action
```cs
//new method (same as Url.Action)
this.ActionUrl<SomeController>(p=>p.Search("sean"));

//before
this.RedirectToAction("Search","Some",new {word="sean"});
//after
this.RedirectToAction<SomeController>(p=>p.Search("sean"));

//if just redirect action in same controller 
this.RedirectToAction(p=>p.Search("sean"));
```
