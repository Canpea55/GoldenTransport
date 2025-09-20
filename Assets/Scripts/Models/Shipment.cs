using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Shipment
{
    public int id;
    public string remark;
    public string vehicle_name;
    public string vehicle_color_hex;
    public string driver_name;
    public List<Order> orders;
}