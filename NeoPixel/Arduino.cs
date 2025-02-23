using Serilog;
using System.Drawing;
using System.IO.Ports;

namespace NeoPixel;

public sealed class Arduino
{
    public ILogger? Logger { get; set; }
    private readonly SerialPort _serial;

    public Arduino(string comPort, int baudRate)
    {
        _serial = new(comPort, baudRate);
        _serial.Open();
    }

    public bool SetPixel(int index, Color color) => SetPixel(index, color.R, color.G, color.B);
    public bool SetPixel(int index, int r, int g, int b) => SaveWrite($"sp:{index},{r},{g},{b};");

    public bool SetStrip(int index, Color color) => SetStrip(index, color.R, color.G, color.B);
    public bool SetStrip(int index, int r, int g, int b) => SaveWrite($"ss:{index},{r},{g},{b};");

    public bool SetPixels(params Color[] colors)
    {
        try
        {
            for (int i = 0; i < colors.Length; i++)
                SetPixel(i + 1, colors[i]);

            Logger?.Information("Read-Text  : {text}", _serial.ReadLine());
            return true;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Could not send serial data.");
            return false;
        }
    }

    private bool SaveWrite(string text)
    {
        try
        {
            _serial.Write(text);
            Logger?.Information("Serial-Text: {text}.", text);
            return true;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Could not send serial data.");
            return false;
        }
    }

    private void Show() => _serial.Write("sw");
}
