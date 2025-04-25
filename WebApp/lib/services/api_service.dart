import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:credential_leakage_monitoring/models/create_customer_model.dart';
import 'package:credential_leakage_monitoring/models/customer_model.dart';
import 'package:credential_leakage_monitoring/models/leak_model.dart';

class ApiService {
  static const String apiBaseUrl = String.fromEnvironment('API_URL');

  factory ApiService() => _instance;

  ApiService._internal();

  static final ApiService _instance = ApiService._internal();

  final _client = http.Client();

  Future<List<LeakModel>> queryLeaksByMailAddress(String emailAddress) {
    final url = '$apiBaseUrl/query?email=${Uri.encodeComponent(emailAddress)}';
    return _getList<LeakModel>(url, (json) => LeakModel.fromJson(json));
  }

  Future<List<CustomerModel>> getCustomers() {
    final url = '$apiBaseUrl/customers';
    return _getList<CustomerModel>(url, (json) => CustomerModel.fromJson(json));
  }

  Future<void> createCustomer(CreateCustomerModel model) {
    final url = '$apiBaseUrl/customers';
    return _send('POST', url, body: model.toJson(), expectedStatus: 201);
  }

  Future<void> updateCustomer(String id, CustomerModel model) {
    final url = '$apiBaseUrl/customers/$id';
    return _send('PUT', url, body: model.toJson(), expectedStatus: 200);
  }

  Future<void> deleteCustomer(String id) {
    final url = '$apiBaseUrl/customers/$id';
    return _send('DELETE', url, expectedStatus: 204);
  }

  Future<List<LeakModel>> queryLeaksForCustomer(String customerId) {
    final url = '$apiBaseUrl/customers/$customerId/query';
    return _getList<LeakModel>(url, (json) => LeakModel.fromJson(json));
  }

  Future<T> _handleResponse<T>(
    http.Response response,
    T Function(dynamic) fromJson,
  ) async {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) {
        return Future.value(null) as T;
      }
      final jsonResponse = jsonDecode(response.body);
      return fromJson(jsonResponse);
    } else {
      throw Exception('Request failed with status: ${response.statusCode}');
    }
  }

  Future<List<T>> _getList<T>(
    String url,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    final response = await _client.get(Uri.parse(url));
    return _handleResponse<List<T>>(response, (json) {
      return (json as Iterable)
          .map((e) => fromJson(e as Map<String, dynamic>))
          .toList();
    });
  }

  Future<void> _send(
    String method,
    String url, {
    dynamic body,
    int expectedStatus = 200,
  }) async {
    http.Response response;
    final headers = {'Content-Type': 'application/json'};
    final uri = Uri.parse(url);

    switch (method) {
      case 'POST':
        response = await _client.post(
          uri,
          headers: headers,
          body: jsonEncode(body),
        );
        break;
      case 'PUT':
        response = await _client.put(
          uri,
          headers: headers,
          body: jsonEncode(body),
        );
        break;
      case 'DELETE':
        response = await _client.delete(uri, headers: headers);
        break;
      default:
        throw Exception('Unsupported HTTP method');
    }

    if (response.statusCode != expectedStatus) {
      throw Exception('Request failed with status: ${response.statusCode}');
    }
  }
}
