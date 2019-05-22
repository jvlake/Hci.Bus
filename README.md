# Hci.Bus

Hci.Bus is a simple pub/sub system that works 'in process'.

Installation:
```
Install-Package Hci.Bus -Version 1.0.3
```

usage:
```
public class Foo{
    public string Text { get; set; }
}

IHciBus bus = new HciBus();
bus.Subscribe<Foo>(p => Console.Write($"Result was {p.Text}"));
bus.Publish(new Foo { Text = "Bar" });
```

Written against .net core 2

License is: GPLv3
