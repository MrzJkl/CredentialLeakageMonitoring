import 'package:credential_leakage_monitoring/models/leak_model.dart';
import 'package:credential_leakage_monitoring/services/api_service.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class QueryScreen extends StatefulWidget {
  const QueryScreen({super.key});

  @override
  State<QueryScreen> createState() => _QueryScreenState();
}

class _QueryScreenState extends State<QueryScreen> {
  final TextEditingController _emailController = TextEditingController();
  bool _hasSearched = false;
  bool _isLoading = false;
  List<LeakModel> _leaks = [];

  Future<void> _checkEmailForLeaks() async {
    final email = _emailController.text.trim();
    if (email.isEmpty) return;

    setState(() {
      _isLoading = true;
      _hasSearched = true;
    });

    try {
      final leaks = await ApiService().queryLeaksByMailAddress(email);
      setState(() {
        _leaks = leaks;
      });
    } catch (e) {
      print('Error: $e');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  Widget _buildResults() {
    if (_isLoading) {
      return const CircularProgressIndicator();
    }

    if (!_hasSearched) return const SizedBox();

    if (_leaks.isEmpty) {
      return Column(
        children: const [
          Icon(Icons.celebration, color: Colors.green, size: 80),
          Text("No leaks found 🎉"),
        ],
      );
    }

    return DataTable(
      columns: const [
        DataColumn(label: Text('Password-Hash')),
        DataColumn(label: Text('First Seen')),
        DataColumn(label: Text('Last Seen')),
      ],
      rows:
          _leaks.map((leak) {
            return DataRow(
              cells: [
                DataCell(Text(leak.passwordHash)),
                DataCell(Text(DateFormat('dd.MM.yyyy').format(leak.firstSeen))),
                DataCell(Text(DateFormat('dd.MM.yyyy').format(leak.lastSeen))),
              ],
            );
          }).toList(),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(32.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text(
                "Check if your email address is in a data breach",
                style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 16),
              SizedBox(
                width: 400,
                child: TextField(
                  controller: _emailController,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    labelText: 'Email address',
                    prefixIcon: Icon(Icons.email),
                  ),
                  keyboardType: TextInputType.emailAddress,
                  onSubmitted: (_) => _checkEmailForLeaks(),
                ),
              ),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _checkEmailForLeaks,
                icon: const Icon(Icons.search),
                label: const Text("Check"),
              ),
              const SizedBox(height: 32),
              _buildResults(),
            ],
          ),
        ),
      ),
    );
  }
}
