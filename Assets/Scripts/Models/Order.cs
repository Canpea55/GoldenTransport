using System;

[Serializable]
public class Order
{
    public int id;
    public string docuno;
    public string custname;
    public string remark;
    public string status;
    public PivotData pivot;
}

[Serializable]
public class PivotData {
    public int shipment_id;
    public int order_id;
    public int list_number;
}

[Serializable]
public class OrderList
{
    public Order[] orders;
}