using Microsoft.Maui.Controls;

namespace HWAY.Payroll.Services.Rendering;
public interface IViewRenderer
{   Task<byte[]> RenderToImageAsync(View view);
}