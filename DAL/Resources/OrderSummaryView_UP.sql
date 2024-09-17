CREATE VIEW View_OrderSummary AS

SELECT o.Id, o.[DateTime], COUNT(p.Id) AS Count
FROM [Orders] as o
JOIN [Products] as p ON o.Id = p.OrderId
GROUP BY o.Id, o.[DateTime]
