using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Shipment
{
    public int id;
    public string remark;
    public int vehicle_id;
    public string vehicle_name;
    public string vehicle_color_hex;
    public int driver_id;
    public string driver_name;
    public string delivery_date;
    public List<Order> orders;
}