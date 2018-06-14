using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface IApp
    { 
        void main_Exit();

        void main_Hide();

        void main_Show();

        string main_GetName();

        int main_GetWidth();

        int main_GetHeight();
    }
}
