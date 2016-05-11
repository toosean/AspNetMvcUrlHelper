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
