﻿namespace FileTransferBatch
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "transferEncyclopediaDataToBookstore":
                    BookLibrary.TransferEncyclopediaDataToBookstore();
                    break;
            }
        }
    }
}
