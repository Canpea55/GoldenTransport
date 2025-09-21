using System;
using System.Collections.Generic;

public interface IOverlayWithSubmit
{
    void SetSubmitCallback(Action<Dictionary<string, object>> callback);
}