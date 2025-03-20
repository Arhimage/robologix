using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System;

[Serializable]
[XmlInclude(typeof(ZoneMovable))]
[XmlInclude(typeof(ZoneDirectional))]
public class ZoneBase : ICloneable
{
    [SerializeField] private string _name;
    [SerializeField] private Color _color;
    [SerializeField] private Vector2 _physicalSize;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            Update();
        }
    }

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            Update();
        }
    }

    public Vector2 PhysicalSize
    {
        get => _physicalSize;
        set
        {
            _physicalSize = value;
            Update();
        }
    }

    public void Fill(ZoneBase zone, bool update = true)
    {
        this._name = zone.Name;
        this._color = zone.Color;
        this._physicalSize = zone.PhysicalSize;
        if (update)
            Update();
    }

    public void Update()
    {
        ZoneChanged?.Invoke();
    }

    public virtual object Clone()
    {
        return new ZoneBase()
        {
            Name = this.Name,
            Color = this.Color,
            PhysicalSize = this.PhysicalSize,
        };
    }

    [XmlIgnore]
    [NonSerialized]
    public ZoneChangedEvent ZoneChanged;
}

[Serializable]
[XmlInclude(typeof(ZoneDirectional))]
public class ZoneMovable : ZoneBase
{
    [SerializeField] private Vector2 _physicalPosition;

    public Vector2 PhysicalPosition
    {
        get => _physicalPosition;
        set
        {
            _physicalPosition = value;
            Update();
        }
    }

    public void Fill(ZoneMovable zone, bool update = true)
    {
        base.Fill(zone, false);
        this._physicalPosition = zone.PhysicalPosition;
        if (update)
            Update();
    }

    public override object Clone()
    {
        return new ZoneMovable()
        {
            Name = this.Name,
            Color = this.Color,
            PhysicalSize = this.PhysicalSize,
            PhysicalPosition = this.PhysicalPosition,
        };
    }
}

[Serializable]
public class ZoneDirectional : ZoneMovable
{
    private bool _isVertical;

    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            _isVertical = value;
            ZoneChanged?.Invoke();
        }
    }

    public void Fill(ZoneDirectional zone, bool update = true)
    {
        base.Fill(zone, false);
        this._isVertical = zone.IsVertical;
        if (update)
            Update();
    }

    public override object Clone()
    {
        return new ZoneDirectional()
        {
            Name = this.Name,
            Color = this.Color,
            PhysicalSize = this.PhysicalSize,
            PhysicalPosition = this.PhysicalPosition,
            IsVertical = this.IsVertical,
        };
    }
}

[Serializable]
public class WarehouseSettings
{
    public ZoneBase Warehouse;
    public List<ZoneMovable> Zones = new List<ZoneMovable>();
}

public delegate void ZoneChangedEvent();

