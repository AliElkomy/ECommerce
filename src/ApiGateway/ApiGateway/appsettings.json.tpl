{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "products": { "ClusterId": "products", "Match": { "Path": "/api/products/{**catch-all}" } },
      "orders": { "ClusterId": "orders", "Match": { "Path": "/api/orders/{**catch-all}" } }
    },
    "Clusters": {
      "products": {
        "Destinations": {
          {{range service "ProductService"}}
          "{{.ID}}": { "Address": "http://{{.Address}}:{{.Port}}/" },
          {{end}}
        }
      },
      "orders": {
        "Destinations": {
          {{range service "OrderService"}}
          "{{.ID}}": { "Address": "http://{{.Address}}:{{.Port}}/" },
          {{end}}
        }
      }
    }
  }
}