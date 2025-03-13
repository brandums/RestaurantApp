using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace RestauranteMap.Models
{
    public class StructureService
    {
        public int Mesas { get; set; } = 10;
        public int TiempoEntrega { get; set; } = 15;
        public User User { get; set; }
        public OrdenPorUser Orden { get; set; }
        public List<OrdenPorUser> OrdenesPorMesa { get; set; }
        public List<Category> Categories { get; set; }
        public PrincipalStruct PrincipalStruct { get; set; }
        public List<Platos> RegularFoods { get; set; }
        public List<Platos> SpecialFoods { get; set; }
        public List<Platos> AllFoods { get; set; }

        private readonly HttpClient _httpClient;
        private readonly string apiUrl = "http://server.com/api";
        private readonly string PrincipalStructFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "principalStruct.json");
        private readonly string UserFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "user.json");

        public StructureService()
        {
            _httpClient = new HttpClient();

            LoadLocalUserData();
            LoadLocalPrincipalStructData();
            loadDishesList();
            GetCategories();
            _ = LoadOrdersByTable();
            Orden = new OrdenPorUser();
        }

        private void LoadLocalUserData()
        {
            if (File.Exists(UserFilePath))
            {
                string userJson = File.ReadAllText(UserFilePath);
                User = JsonConvert.DeserializeObject<User>(userJson);
            }
        }

        private void LoadLocalPrincipalStructData()
        {
            if (File.Exists(PrincipalStructFilePath))
            {
                string principalStructJson = File.ReadAllText(PrincipalStructFilePath);
                PrincipalStruct = JsonConvert.DeserializeObject<PrincipalStruct>(principalStructJson);
            }
        }

        public async Task<bool> CheckAndUpdateData()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await UpdateUserData();
                await UpdatePrincipalStructData();
                loadDishesList();
                GetCategories();

                await LoadOrdersByTable();

                return true;
            }

            return false;
        }

        public async Task<List<User>> LoadUsersAsync(string tipo)
        {
            var users = new List<User>();
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/User/{tipo}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    return users = JsonConvert.DeserializeObject<List<User>>(responseJson);
                }
                else
                {
                    return users = new List<User>();
                }
            }
            catch (Exception ex)
            {
                return users = new List<User>();
            }
        }

        public async Task LoadOrdersByTable()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/Plato/byTable");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    OrdenesPorMesa = JsonConvert.DeserializeObject<List<OrdenPorUser>>(responseJson);
                }
                else
                {
                    OrdenesPorMesa = new List<OrdenPorUser>();
                }
            }
            catch (Exception ex)
            {
                OrdenesPorMesa = new List<OrdenPorUser>();
            }
        }


        public async Task UpdateUserData()
        {
            if (User == null) return;

            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/User/{User.Id}");
                if (response.IsSuccessStatusCode)
                {
                    string updatedUserJson = await response.Content.ReadAsStringAsync();
                    File.WriteAllText(UserFilePath, updatedUserJson);
                    User = JsonConvert.DeserializeObject<User>(updatedUserJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el usuario: {ex.Message}");
            }
        }

        public async Task UpdatePrincipalStructData()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/PrincipalStruct");
                if (response.IsSuccessStatusCode)
                {
                    string updatedPrincipalStructJson = await response.Content.ReadAsStringAsync();
                    File.WriteAllText(PrincipalStructFilePath, updatedPrincipalStructJson);
                    PrincipalStruct = JsonConvert.DeserializeObject<PrincipalStruct>(updatedPrincipalStructJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar la estructura principal: {ex.Message}");
            }
        }

        private void loadDishesList()
        {
            if (PrincipalStruct != null)
            {
                var platosList = new List<Platos>();

                var substruct = PrincipalStruct?.Substructs[0];
                if (substruct != null)
                {
                    for (int i = 0; i < substruct.Nombre?.Length; i++)
                    {
                        var plato = new Platos
                        {
                            Nombre = substruct.Nombre[i],
                            Precio = substruct.Precio?[i],
                            Descripcion = substruct.Descripcion?[i],
                            Codigo = substruct.Codigo?[i],
                            Categoria = substruct.Categoria?[i],
                            Image = substruct.Images?[i]?.FirstOrDefault() ?? "",
                            Images = substruct.Images?[i],
                            Especial = (substruct.Extra1?[i] == "1") ? true : false,
                            Receta = JsonConvert.DeserializeObject<Receta>(substruct.Extra8?[i])
                        };

                        platosList.Add(plato);
                    }
                }

                RegularFoods = new List<Platos>(platosList.Where(p => !p.Especial));
                SpecialFoods = new List<Platos>(platosList.Where(p => p.Especial));
                AllFoods = new List<Platos>(platosList);
            }
        }

        public void GetCategories()
        {
            if (PrincipalStruct != null)
            {
                var categoriesList = new List<Category>();
                for (int i = 0; i < PrincipalStruct.Data6.Length; i++)
                {
                    var category = new Category
                    {
                        Name = PrincipalStruct.Data6[i],
                        Logo = PrincipalStruct.Data7[i],
                        Image = PrincipalStruct.Data8[i]
                    };
                    categoriesList.Add(category);
                }

                var orderedCategories = categoriesList
                    .OrderByDescending(c => !string.IsNullOrEmpty(c.Image))
                    .ToList();

                Categories = categoriesList;
            }
        }

        public async Task<bool> SendPlatosWithQuantity(OrdenPorUser order)
        {
            order.Platos = AllFoods?.Where(p => p.Quantity > 0).ToList();

            if (order.Platos == null || !order.Platos.Any())
            {
                return false;
            }
            var json = JsonConvert.SerializeObject(order);
            try
            {
                var response = await _httpClient.PostAsync($"{apiUrl}/Plato", new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    LoadOrdersByTable();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<OrdenPorUser> GetOrder(string code)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/Plato/byCode/{code}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var pedido = JsonConvert.DeserializeObject<OrdenPorUser>(responseJson);
                    return pedido;
                }
                else
                {
                    Console.WriteLine($"Error al obtener pedidos pendientes: {response.StatusCode}");
                    return new OrdenPorUser();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar obtener pedidos pendientes: {ex.Message}");
                return new OrdenPorUser();
            }
        }

        public async Task<List<OrdenPorUser>> GetOrders(int state)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/Plato/{state}");

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    var pedidosPendientes = JsonConvert.DeserializeObject<List<OrdenPorUser>>(responseJson);
                    return pedidosPendientes;
                }
                else
                {
                    Console.WriteLine($"Error al obtener pedidos pendientes: {response.StatusCode}");
                    return new List<OrdenPorUser>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar obtener pedidos pendientes: {ex.Message}");
                return new List<OrdenPorUser>();
            }
        }

        public async Task<bool> UpdatePlatosStatus(int state, OrdenPorUser orden)
        {
            if (orden == null || orden.Platos == null || !orden.Platos.Any())
            {
                return false;
            }

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(orden), Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{apiUrl}/Plato/changeStateOrder/{state}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var content = new MultipartFormDataContent();
            var imageContent = new StreamContent(imageStream);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            content.Add(imageContent, "image", fileName);

            var response = await _httpClient.PostAsync($"{apiUrl}/PrincipalStruct/UploadImage", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al subir la imagen: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            return result.GetProperty("imageUrl").GetString();
        }

        public async Task<bool> UpdateUserInfo(User user)
        {
            try
            {
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{apiUrl}/User/{user.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<int?> GetCalificationAsync(int userId, int meseroId)
        {
            var response = await _httpClient.GetAsync($"{apiUrl}/User/getCalification/{userId}/{meseroId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int?>();
        }

        public async Task AddCalificationAsync(Calification calification)
        {
            var response = await _httpClient.PostAsJsonAsync($"{apiUrl}/user/addCalification", calification);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<string>> GetTableStatesAsync()
        {
            var response = await _httpClient.GetAsync($"{apiUrl}/Plato/getTableStates");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<string>>();
                return result ?? new List<string>();
            }

            throw new Exception($"Error: {response.ReasonPhrase}");
        }

        public async Task<string> ToggleTableStateAsync(int position, string value)
        {
            var response = await _httpClient.GetAsync($"{apiUrl}/Plato/toggleTable/{position}/{value}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                return result?.NewValue ?? "Error";
            }

            throw new Exception($"Error: {response.ReasonPhrase}");
        }

        public async Task<bool> UpdateUser(User user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{apiUrl}/User/{user.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al actualizar el usuario: {response.StatusCode} - {error}");
        }

        public async Task<OrdenPorUser> UpdateFacturacionAsync(OrdenPorUser orden)
        {
            try
            {
                string json = JsonConvert.SerializeObject(orden);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{apiUrl}/User", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var updatedOrden = JsonConvert.DeserializeObject<OrdenPorUser>(responseBody);

                    return updatedOrden;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en la solicitud: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<AlmacenList> GetAlmacenes()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{apiUrl}/Almacen/almacenes");
                if (response.IsSuccessStatusCode)
                {
                    var almacenes = await response.Content.ReadFromJsonAsync<AlmacenList>();
                    if (almacenes != null)
                    {
                        return almacenes;
                    }
                    else
                    {
                        Console.WriteLine("Error: El contenido recibido es nulo.");
                        return new AlmacenList();
                    }
                }
                else
                {
                    Console.WriteLine($"Error al obtener los estados de las mesas: {response.StatusCode}");
                    return new AlmacenList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al intentar obtener los estados de las mesas: {ex.Message}");
                return new AlmacenList();
            }
        }

        public async Task<string> AddRegistroAlmacen(RegistroAlmacen nuevoRegistro)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(nuevoRegistro);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiUrl}/Almacen/registros", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error al añadir el registro: {response.StatusCode}, {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consumir el servicio: {ex.Message}", ex);
            }
        }

        public async Task<string> UpdateStockAsync(string codigo, int cantidad)
        {
            try
            {
                string url = $"{apiUrl}/Almacen/updateAlmacen/{codigo}/{cantidad}";

                var response = await _httpClient.PutAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"Error: {response.StatusCode} - {error}";
                }
            }
            catch (Exception ex)
            {
                return $"Error de conexión: {ex.Message}";
            }
        }

        public async Task<string> AddAlmacenAsync(AlmacenList nuevoAlmacen)
        {
            try
            {
                var json = JsonConvert.SerializeObject(nuevoAlmacen);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiUrl}/Almacen/almacenes", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Error al agregar el almacén: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Excepción al agregar almacén: {ex.Message}", ex);
            }
        }

        public async Task<bool> CrearPlato(PlatoDTO plato)
        {
            var json = JsonConvert.SerializeObject(plato);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{apiUrl}/Plato/create-plato", content);

            if (response.IsSuccessStatusCode)
            {
                await UpdatePrincipalStructData();
                loadDishesList();
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
            return false;
        }

        public async Task<bool> EditarPlato(PlatoDTO plato)
        {
            var json = JsonConvert.SerializeObject(plato);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{apiUrl}/Plato/update-plato", content);

            if (response.IsSuccessStatusCode)
            {
                await UpdatePrincipalStructData();
                loadDishesList();
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
            return false;
        }

        public async Task<bool> EliminarPlato(string codigo)
        {
            var response = await _httpClient.DeleteAsync($"{apiUrl}/Plato/delete-plato/{codigo}");

            if (response.IsSuccessStatusCode)
            {
                await UpdatePrincipalStructData();
                loadDishesList();
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {error}");
            return false;
        }

        public async Task<bool> CreateOrUpdateCategory(string value, Category category)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(category);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{apiUrl}/PrincipalStruct/createCategory/{value}", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    await UpdatePrincipalStructData();
                    GetCategories();
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error en la solicitud: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear o actualizar la categoría: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int index)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{apiUrl}/PrincipalStruct/deleteCategory/{index}", null);

                if (response.IsSuccessStatusCode)
                {
                    await UpdatePrincipalStructData();
                    GetCategories();
                    return true;
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al eliminar la categoría: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción: {ex.Message}");
                return false;
            }
        }

    }
}
