import 'dart:convert';

import 'package:credentialleakagemonitoring/constants.dart';
import 'package:credentialleakagemonitoring/models/leak_model.dart';
import 'package:http/http.dart' as http;

class ApiService {
  Future<List<LeakModel>> queryLeaksByMailAddress(String emailAddress) async {
    final response = await http.get(
      Uri.parse(
        '${Constants.baseUrl}/query?email=${Uri.encodeComponent(emailAddress)}',
      ),
    );

    if (response.statusCode == 200) {
      final jsonResponse = jsonDecode(response.body) as Iterable;
      return jsonResponse
          .map((json) => LeakModel.fromJson(json as Map<String, dynamic>))
          .toList();
    } else {
      throw Exception('Failed to load leaks');
    }
  }
}
